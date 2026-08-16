# Security review of XMPPConsole and Ratatoskr

**Date:** 2026-08-14
**Scope:** XMPPConsole (console UI) and the XMPP implementation in the Ratatoskr submodule, with brief notes on the bundled `XMPPServer`.
**Focus:** Protocol correctness, spoofing, authentication, transport, OMEMO, and denial of service.

This is a code review, not a live penetration test. Findings come from reading the client, the protocol library, and the server that ships with Ratatoskr.

> **The ✅ marks were added afterwards.** One means the point is closed in this
> repository and the fix is under test; where only part of a finding is closed,
> the mark sits on that part and nowhere else. Apart from those marks the text
> is the document as it arrived — including the places where reading the code
> disagreed with it, which are answered in the working notes rather than
> silently edited in here.

---

## Overall assessment

XMPPConsole is only the front end. Authentication, stanza routing, spoofing checks and OMEMO live in `libs/Ratatoskr`. For experimental software the library is unusually security-conscious: many classic XMPP traps (roster pushes, carbons, receipts, PubSub senders, the SCRAM server signature, SASL pinning) are recognised and defended.

Several remaining issues are already documented honestly in the Ratatoskr README (no channel binding, `ws://` not refused, plaintext OMEMO store, SASL trust-on-first-use). The findings below are **additional gaps**, or places where an existing defence is incomplete.

The most serious protocol bug is not in the cryptography. It is **IQ correlation without a sender check** — exactly on the path where OMEMO fetches its keys.

---

## What is already in good shape

These belong in the report so the list of problems does not make the code look worse than it is.

- SCRAM checks the nonce prefix and the server signature in constant time. An empty `<success/>` aborts the handshake.
- SASLprep is complete: mapping, NFKC, prohibited tables, unassigned code points, and the bidi rules.
- SASL pinning after the first successful login. The lower bound is checked **before** a PLAIN `<auth/>` goes out.
- Roster pushes are accepted only without a `from` or from the account’s own bare JID. A spoofed push is not answered (presence leak).
- Carbons, receipts and PubSub events have sender checks (the OMEMO-carbon path is the exception; see below).
- Entity capabilities: `ver` is recomputed; a mismatch is not stored in the cache.
- OMEMO never falls back to plaintext. A changed identity key is rejected. Small-order Curve25519 points are refused.
- XML escaping is centralised. Incoming frames are dispatched by element name, not by `StartsWith("<iq")`.
- The account store keeps salt, StoredKey and ServerKey, not the password.
- Many of the traps above already have tests (`RosterPushSecurityTests`, `AccountEnumerationTests`, OMEMO interop against python-omemo).

---

## Critical / high

### 1. ✅ IQ replies accepted without a `from` check

*Closed in `efb86e5` for `SendIqAsync`: every pending request carries whom it was addressed to, and an answer naming a sender has to name that one. An answer naming nobody still passes — RFC 6120, section 8.1.2.1 makes the server stamp the real sender onto anything a client sends, so a peer cannot produce one. Two parts of this finding are **not** closed: `DiscoManager` and `PingManager` keep pending maps of their own, and the identifiers stayed countable, because the comparison is the fix and random identifiers only raise the price of guessing.*

`SendIqAsync` remembers only the stanza `id`. `TryCompleteIq` completes the first matching `result` or `error` IQ, **regardless of `from`**.

```csharp
// XMPPConnection.ProcessIq
if (id is not null && type is "result" or "error" && TryCompleteIq(id, element))
    return;
```

The identifiers are predictable: `carbons-enable`, `roster1`, `sess1`, `pep-1`, `pep-2`, `disco-info-1`. The full JID is broadcast in presence (`console-<pid>`).

**Attack.** While the victim fetches a PEP item, another contact sends:

```xml
<iq type='result' id='pep-1' from='mallory@evil.example' to='alice@example.com/console-12345'>
  <!-- forged OMEMO bundle -->
</iq>
```

`FetchPepAsync` then only checks `type='result'`, not the sender. A **foreign OMEMO bundle can be substituted**. The client encrypts to Mallory’s keys while believing the message is end-to-end encrypted to Alice.

The same pattern applies to the roster fetch, carbons enable, disco and ping. `DiscoManager.ProcessInfoResult` stores `from` without checking that it is the JID that was queried.

This is the highest-severity protocol error. XEP-0384 exists to protect against the server and the network. Here another client on the same server can swap the key.

**Fix.** Store the expected `from` (or “own server / no `from`”) with each pending IQ and compare it before `TrySetResult`. Make IDs random (`Guid`), not `pep-1`.

---

### 2. Endpoint discovery: HTTPS may redirect to HTTP; `wss://` host is not bound to the JID domain

`AltConnectionsResolver` checks only the **initial** URL for `https://`. The shared `HttpClient` follows redirects by default, including to `http://`.

The first `wss://` link in the document is then taken over — **without** checking that its host belongs to the JID domain.

Chain:

1. `https://example.com/.well-known/host-meta.json` returns 302 to `http://…`
2. A man in the middle on the cleartext hop injects `wss://attacker.example/ws`
3. The client authenticates there (SCRAM proof, or PLAIN)

XEP-0156 requires HTTPS *and* secure endpoints. A redirect undoes the first half.

**Fix.** Follow redirects only to `https://`. Check the final response URI. Bind the `wss://` host to the JID domain, or to an explicit allow-list.

---

### 3. ✅ `ws://` is not refused

*Closed in `fd398db`. `EndpointPolicy` refuses a `ws://` endpoint from `-w` and from the prompt alike, `--insecure` is the way to say otherwise, and the console now sets `MinimumSaslMechanism` to `SCRAM-SHA-256` by default, lowerable with `--sasl`.*

An endpoint given with `-w`, typed at the prompt, or left as a default is connected as-is. Over cleartext WebSocket:

- The first login is not yet protected by SASL pinning.
- The console never sets `MinimumSaslMechanism`.
- A man in the middle offers only PLAIN → the password goes out in the clear.

Ratatoskr’s README already states this. The console does not stop it.

**Fix.** Refuse `ws://` in the client. Default `MinimumSaslMechanism` to `SCRAM-SHA-256`.

---

### 4. ✅ SCRAM iteration count has no lower or upper bound

*Closed in `3b758f2`: digits only, and between 4096 and a million. Measured rather than argued — against the old line `i=2147483647` kept the process four minutes and one second inside the derivation. The negative case turned out to be the third door: `int.Parse` accepted `-1` and PBKDF2 then threw an `ArgumentOutOfRangeException` out of the middle of the handshake, so the report's "can fail hard or burn CPU" was the former.*

```csharp
// SCRAMAuthenticator.ProcessServerFirstMessage
var salt = Convert.FromBase64String(saltBase64);
var iterations = int.Parse(iterationsStr);
_saltedPassword = Hi(_password, salt, iterations);
```

RFC 7677 requires at least 4096 iterations for SCRAM-SHA-256. Neither bound exists here.

| Value | Effect |
|---|---|
| `i=1` | Offline brute-force becomes cheap (MITM or compromised server) |
| `i=2147483647` | Login spends a very long time in PBKDF2 — denial of service |

`int.Parse` throws on overflow. A negative value reaches `Pbkdf2` and can fail hard or burn CPU.

**Fix.** Reject `i` below 4096 (SHA-256; same floor is reasonable for SHA-1 here). Cap the maximum (for example 1_000_000).

---

## Medium

### 5. Encrypted carbons bypass the spoofing check

XEP-0280: carbons MUST come only from the account’s own bare JID. `CarbonManager` enforces that. The OMEMO branch runs **before** it and without that check:

```csharp
// XMPPConnection.ProcessMessage
if (Omemo is not null &&
    element.HasNamespace(CarbonManager.Namespace) &&
    element.Descendants()
           .FirstOrDefault(e => e.Name.LocalName     == "forwarded" &&
                                e.Name.NamespaceName == "urn:xmpp:forward:0")
          ?.Elements()
           .FirstOrDefault(e => e.Name.LocalName == "message") is XElement wrapped &&
    (wrapped.Attr("from") ?? wrapped.Attr("to")) is String innerSender &&
    TryProcessEncrypted(wrapped, innerSender))
{
    return;
}
```

Further weaknesses on the same path:

- `HasNamespace` matches any descendant in the carbons namespace.
- `Descendants()` finds any nested `<forwarded/>`.
- `SceEnvelope.TryRead` skips the sender check when the envelope has **no** `<from/>` (`from is not null`).
- Encryption never sets `to`, so there is no recipient binding.

Full impersonation usually still fails because the ratchet session is keyed to the inner sender. The path is nevertheless a clear XEP-0280 violation and an opening for replay, session disruption and a confused UI.

**Fix.** Run the carbon spoofing check first, then decrypt. In `SceEnvelope.TryRead`, reject a missing `<from/>` when `expectedFrom` is set. Set `to` when encrypting.

---

### 6. No stanza / frame size limit

Neither client nor server defines a `MaxStanza`. The receive loop appends into a `StringBuilder` with no cap. `XmlStreamSplitter.rest` grows without bound as well.

A peer can exhaust process memory with a single WebSocket or TCP frame. RFC 6120 §13.12 expects an upper bound.

**Fix.** Cap the frame size (for example 1–4 MiB) on both receive paths and in `XmlStreamSplitter`.

---

### 7. OMEMO operations are weaker than the cryptography

The crypto itself (X3DH, double ratchet, payload AES, small-order check, `MaxSkip = 1000`, bundle signature) is careful and tested against python-omemo. The surrounding operations are not at the same level.

- **The store is plaintext** next to the executable (`omemo-user_example.com.json`), including the identity key and every chain key. File mode is not set to `0600`. Anyone who can read the file can read the conversations.
- ✅ **Prekeys are consumed and the bundle is not republished.** `TakePreKey` updates local state but does not publish again. Used prekeys stay in the public bundle; new ones are not added. XEP-0384 requires replenishment. *(Refilled and republished on every consumption. It was worse than a missing MUST: `X3DH.Accept` throws on a spent prekey, so the second stranger to reach into a stale bundle got a first message nobody could read.)*
- **The signed prekey is never rotated on a schedule.** Without rotation, compromise of the current SPK undoes part of the forward secrecy the rotation exists for.
- **`TrustNewDevicesBlindly = true`.** Blind trust before verification. A deliberate trade-off, but a new device is accepted until someone compares fingerprints.
- ✅ **`OmemoManager` is barely serialised.** Encrypt and decrypt can run in parallel (`Task.Run` in `TryProcessEncrypted`); only `BuildSessionAsync` takes the lock. Two concurrent messages can read the same ratchet state and make one message unreadable. *(One semaphore per bare JID and device now spans the whole load-to-save. The report is right that it makes one message unreadable — and the test proving it fails deterministically, not by timing.)*

**Fix.** Create the store with mode `0600`; optionally protect it with DPAPI / `ProtectedData`. Republish the bundle after consuming a prekey. Rotate the signed prekey. Serialise encrypt/decrypt on the same session.

---

### 8. SASL: no channel binding, PLAIN as last resort, pinning only from the second login

- No `SCRAM-SHA-256-PLUS` / `tls-exporter` (RFC 9266). A TLS man in the middle with a trusted certificate (compromised CA, mis-issued cert) sees the mechanism list and can force PLAIN — on the **first** connect.
- ✅ The console does not set `MinimumSaslMechanism`. Anyone who knows their server should demand at least `SCRAM-SHA-256`. *(Set, and to exactly that.)*
- The password is kept as a `string` on `XMPPConnection` for the lifetime of the process (not wipeable; survives in heap dumps).

---

### 9. Connection constructor does not parse JIDs per RFC 7622

The constructor splits on `@` and requires exactly two pieces:

```csharp
var parts  = jid.Split('@');
if (parts.Length != 2)
    throw new ArgumentException("The JID has to be in the format 'user@domain'", nameof(jid));

_username  = parts[0];
_domain    = parts[1];
```

`alice@example.com/phone` becomes domain `example.com/phone`. `a@b@c` is rejected even though a resourcepart may contain `@`. PRECIS and IDNA are not applied at this boundary. `JidUtilities` exists and is not used here.

**Fix.** Parse the login JID with `JidUtilities.Parse`.

---

### 10. Server (`XMPPServer`), if anyone runs it for real

- No rate limiting, no account lockout. The README already says so (RFC 6120 §13.11).
- PLAIN is offered by default.
- Authentication is parsed with a regex on the raw frame (`<auth[^>]*>([^<]*)</auth>`), not with the XML parser — brittle and easy to confuse.
- Decoy salts for unknown users change after a restart → account enumeration via the challenge.
- PLAIN has different timing from SCRAM (also documented).
- `FileAccountStore` writes StoredKey / ServerKey in the clear, without restricting file permissions.
- `Ed25519Math` is not constant-time (`BigInteger`, bit-dependent branches). Acceptable for a local client; a side channel if a server computes OMEMO signatures for remote peers.

---

## Low / design

| Area | Issue |
|---|---|
| PEP device list | `ProcessPepEventAsync` still runs after `PubSub.ProcessEvent` has rejected the sender. Re-publish only happens when `from` is the own bare JID — that relies on the server overwriting `from`. |
| PubSub authorisation form | Any message with a matching data form raises `OnPubSubSubscriptionRequest`, with no check that it came from the account’s own PubSub service. Social engineering toward `/pubsub request`. |
| Auto receipts / auto markers | Every 1:1 message with a request is acknowledged automatically. That confirms the resource is reachable. Groupchat is correctly excluded. |
| Chat markers | No spoofing check comparable to receipts. A third party can fake “read” for someone else’s message. |
| Resource `console-<pid>` | Leaks the process id, makes the full JID guessable, collides if two clients share a process. |
| `/raw` | Prints bodies and auth frames (documented). |
| `-p` on the command line | Lands in the shell history and in `ps` (documented). |
| `launchSettings.json` | Local debugging profile can hold real credentials. It is gitignored — correct — but the file mode is still world-readable on disk. |
| Keepalive / `/sm` | Changes take effect only after a reconnect (documented). After a silent timeout the connection can look alive while it is not. |
| Missing features | No MUC/MIX, no MAM, no HTTP upload, no blocking, no SASL2/Bind 2. Not vulnerabilities; missing modern-client surface. |

---

## Recommended order of work

1. ✅ **`SendIqAsync`:** store the expected `from` (or “own server / no `from`”) and compare it before completing the wait. Use random IDs (`Guid`), not `pep-1`.  *(The comparison is in; the identifiers stayed countable.)*
2. **OMEMO carbons:** spoofing check first, then decrypt. Reject a missing SCE `<from/>` when `expectedFrom` is set. Set `to` on encrypt.
3. **Transport:** ✅ refuse `ws://`. Follow host-meta redirects only to `https://`. Check the final URI. Bind the `wss://` host to the JID domain or an allow-list.
4. ✅ **SCRAM:** reject `i < 4096`; impose a hard maximum (for example 1_000_000).
5. **DoS:** maximum frame size (for example 1–4 MiB) on both receive paths and in `XmlStreamSplitter`.
6. **OMEMO store:** file mode `0600`, optional OS-backed encryption. ✅ Republish prekeys after use. Rotate the signed prekey.
7. **Console:** ✅ default `MinimumSaslMechanism = "SCRAM-SHA-256"`. Accept the JID through `JidUtilities.Parse`.

---

## Architecture notes (for context)

Three layers, cleanly separated:

| Layer | Type | Role |
|---|---|---|
| UI | `Program` | Command line, dispatch, presentation. No protocol logic |
| Application | `XMPPClient` | Session state and composite operations |
| Protocol | `XMPPConnection` + XEPs | WebSocket I/O, SASL, binding, stanza routing, OMEMO |

Only the first lives in this repository; the other two come from Ratatoskr.

**Transport today:** XMPP over WebSocket (RFC 7395), `wss://`. No TCP client path, no BOSH.

**Authentication today:** SCRAM-SHA-256 preferred, SCRAM-SHA-1 as fallback, SASL PLAIN as last resort. No `-PLUS` channel binding.

**Maturity:** the project itself calls the client experimental. The connection works against Prosody 13 over `wss://`. Connection management and error handling are incomplete. The protocol suite lives with Ratatoskr and XMPPConformanceTests, not in this repository’s eight console-output tests.
