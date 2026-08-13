# XMPPConsole

[![CI](https://github.com/Vanaheimr/XMPPConsole/actions/workflows/ci.yml/badge.svg)](https://github.com/Vanaheimr/XMPPConsole/actions/workflows/ci.yml)

An interactive XMPP client for the command line: it logs in to an XMPP server
over WebSocket (RFC 7395), authenticates with SCRAM, and then gives you a
prompt from which you chat, manage your roster, query services and drive
PubSub — everything as a `/command`, everything else you type goes to the
current conversation partner.

The protocol itself does not live here. It lives in
**[Ratatoskr](https://github.com/Vanaheimr/Ratatoskr)**, a library of its own
under `libs/`; XMPPConsole is its hand-operated front end. What is in this
repository is user interface: command line parsing, command dispatch and
presentation.

> **Maturity: experimental.** The client connects, authenticates and chats
> against Prosody 13 over `wss://`. Connection management and error handling
> are incomplete. What is checked in this repository is the console output and
> nothing else — the protocol is checked with the library. Not for production
> use.

---

## Contents

- [What it does](#what-it-does)
- [What it speaks today](#what-it-speaks-today)
- [Requirements](#requirements)
- [Getting started](#getting-started)
- [Command line options](#command-line-options)
- [Commands](#commands)
- [How the connection is made](#how-the-connection-is-made)
- [Keepalive](#keepalive)
- [End-to-end encryption (OMEMO)](#end-to-end-encryption-omemo)
- [Security notes](#security-notes)
- [Architecture](#architecture)
- [Repository layout](#repository-layout)
- [Tests](#tests)
- [License](#license)

---

## What it does

- **Chat.** Set a conversation partner with `/to`, then type — every line
  without a leading `/` goes out as a message. Single messages without
  switching partner go with `/msg`. A message already sent is corrected with
  `/fix` (XEP-0308).
- **Roster.** List, filter, add and remove contacts, see who is online, read
  the details of a single contact, and accept or deny incoming contact
  requests.
- **Presence.** Set your own status (`available`, `away`, `chat`, `dnd`, `xa`)
  with a status text.
- **Typing notifications and read markers.** `/typing`, `/paused`, `/gone`
  (XEP-0085) and `/mark received|displayed|ack` (XEP-0333).
- **Service discovery.** Ask any JID what it can do (`/disco info`), what it
  offers (`/disco items`), and compare the server's feature list against your
  own (`/features`).
- **PubSub.** Roughly two dozen subcommands under `/pubsub`: subscribe,
  publish, fetch items, create, configure and delete nodes, manage roles,
  subscribers and access models, and answer subscription requests (XEP-0060).
- **OMEMO.** End-to-end encrypted messages, fingerprint comparison and
  per-device trust decisions (XEP-0384).
- **Operational insight.** `/ping` with round-trip measurement, `/who`,
  `/carbons`, `/sm`, `/csi`, `/keepalive`, and `/raw` to watch the actual XML
  going back and forth.

The console keeps its input line intact while all of this happens: incoming
messages, presence changes and the log itself go through one output lock, so
nothing is ever written into the middle of what you are typing.

## What it speaks today

Inherited from [Ratatoskr](https://github.com/Vanaheimr/Ratatoskr) and reachable
from this console:

| Area | State |
|---|---|
| Transport | WebSocket over TLS (RFC 7395), `wss://`. No TCP, no BOSH |
| Authentication | SCRAM-SHA-256 preferred, SCRAM-SHA-1 as fallback, SASL PLAIN as the last resort. No channel binding (`-PLUS`) |
| Endpoint discovery | XEP-0156 `host-meta` over HTTPS; only `wss://` endpoints are taken |
| JID handling | RFC 7622 (PRECIS, IDNA2008) |
| XEP-0030 / XEP-0115 / XEP-0128 | Service discovery, entity capabilities, disco extensions |
| XEP-0060 | Publish-Subscribe, including node ownership and access models |
| XEP-0085 / XEP-0184 / XEP-0333 | Chat states, delivery receipts, chat markers |
| XEP-0198 | Stream Management with resumption, on by default |
| XEP-0199 | Ping and RTT measurement |
| XEP-0203 | Delayed delivery — late messages carry their original date |
| XEP-0280 | Message Carbons, with spoofing protection |
| XEP-0308 | Last message correction (`/fix`) |
| XEP-0352 | Client State Indication (`/csi`) |
| XEP-0384 / XEP-0420 | OMEMO 2 (`urn:xmpp:omemo:2`) with stanza content encryption |

Carbons, receipts, PubSub events, roster pushes and caps answers are all
checked against a forged sender before they are processed.

Not implemented: MUC/MIX group chat, MAM history, HTTP file upload, Jingle,
blocking, avatars, in-band registration, SASL2/Bind 2. The full catalogue of
what a modern XMPP client could speak — and where each specification stands —
is in [docs/STANDARDS.md](docs/STANDARDS.md).

## Requirements

- **.NET SDK 10.0** or newer
- An XMPP account on a server that offers **XMPP over WebSocket** (RFC 7395) —
  Prosody, ejabberd and most modern servers do, but it often has to be enabled

## Getting started

The libraries are **sibling checkouts**, not submodules: `XMPPConsole.csproj`
reaches for `..\..\Ratatoskr`, that one for `..\..\Hermod`, and that one for
`..\..\Styx`. So all four go beside each other in one directory:

```bash
git clone https://github.com/Vanaheimr/XMPPConsole.git
git clone https://github.com/Vanaheimr/Ratatoskr.git
git clone https://github.com/Vanaheimr/Hermod.git
git clone https://github.com/Vanaheimr/Styx.git
```

which gives the layout the relative paths assume:

```
XMPPConsole/   Ratatoskr/   Hermod/   Styx/
```

Nothing here pins anything, so this builds against the current master of all
three — a breaking change over there turns this red without anything here
having moved. That is the same arrangement Ratatoskr, Hermod and Styx have
among themselves. If you want a build that cannot drift, take
[XMPPConformanceTests](https://github.com/Vanaheimr/XMPPConformanceTests)
instead: it pins all four as submodules, holds this console in its solution,
and is the one place in the family where a reproducible build lives.

Build:

```bash
dotnet build XMPPConsole.Tests/XMPPConsole.Tests.csproj
```

or open `XMPPConsole.slnx`, which holds the console, its tests and the three
libraries as source rather than as references pointing off-screen. Three of
its five paths leave the repository, so it opens in the layout above and
nowhere else.

Run — with no arguments it asks for JID, password and WebSocket URI
interactively:

```bash
dotnet run --project XMPPConsole
```

Or pass them:

```bash
dotnet run --project XMPPConsole -- -j user@example.com -p secret
```

The built executable is `XMPPConsole` and takes the same options:

```bash
./XMPPConsole/bin/Debug/net10.0/XMPPConsole -j user@example.com -p secret
```

## Command line options

| Option | Meaning |
|---|---|
| `-j`, `--jid <jid>` | JID in the form `user@domain` |
| `-p`, `--password <pw>` | Password |
| `-w`, `--ws`, `--websocket <uri>` | WebSocket endpoint, e.g. `wss://xmpp.example.com:5281/xmpp-websocket` |
| `-v`, `--verbose` | Verbose logging (trace level — shows every stanza) |
| `-h`, `--help` | Show the help and exit |

Anything not given on the command line is asked for at startup. The password
prompt does not echo. Leaving the WebSocket URI empty starts the endpoint
discovery described [below](#how-the-connection-is-made).

```bash
# interactive
dotnet run --project XMPPConsole

# with an explicit endpoint (needed for servers that publish no host-meta)
dotnet run --project XMPPConsole -- -j user@example.com -p pw \
  -w wss://xmpp.example.com:5281/xmpp-websocket

# with the full protocol log
dotnet run --project XMPPConsole -- -j user@example.com -p secret -v
```

## Commands

At the prompt, a line **without** a leading `/` is sent as a message to the
current conversation partner. Everything else is a command.

### Messages

```
/to <jid>                 set the conversation partner (alias: /chat)
/to                       reset the conversation partner
/msg <jid> <text>         send a single message (alias: /m)
/fix <text>               correct the last message to this partner (alias: /corr)
/status [show] [text]     set the status: available|away|chat|dnd|xa (alias: /s)
```

`/fix` takes the **complete new text**, not the change to it. It corrects the
last message to the current partner and becomes the last one itself, so a
correction can be corrected.

### Contacts (roster)

```
/roster [filter]          show the contacts (aliases: /list, /contacts)
/online                   only the contacts that are online
/add <jid> [name] [g1,g2] add a contact and ask for a subscription
/remove <jid>             remove a contact (alias: /del)
/info <jid>               contact details
/groups                   groups with the number of contacts
/pending                  pending contact requests
/accept [jid]             accept a contact request (no argument: the first)
/deny [jid]               deny a contact request (no argument: the first)
```

### Chat states (XEP-0085)

```
/typing                   send 'is typing'
/paused                   send 'has stopped typing'
/gone                     leave the chat
```

### Chat markers (XEP-0333)

```
/mark received [msg-id]   mark as received (alias: r)
/mark displayed [msg-id]  mark as read (aliases: d, read)
/mark ack [msg-id]        acknowledge (aliases: acknowledged, a)
```

Without a `msg-id` the message received last is used.

### Service discovery (XEP-0030)

```
/disco                    show the subcommands
/disco server             features of your own server
/disco info <jid>         features of a JID
/disco items <jid>        services/items of a JID
/features                 the server's features and your own
```

### PubSub (XEP-0060)

```
/pubsub                             show the subcommands
/pubsub sub <node> [jid]            subscribe to a node (alias: subscribe)
/pubsub unsub <node> [jid] [subid]  end a subscription (alias: unsubscribe)
/pubsub subs                        own subscriptions with subid (alias: subscriptions)
/pubsub sync [jid]                  fetch the subscriptions from the service
/pubsub opts <node> [subid]         options of the subscription (alias: options)
/pubsub deliver <node> <on|off> [subid]   delivery on/off
/pubsub pub <node> <id> <data>      publish an item (alias: publish)
/pubsub get <node> [max]            fetch items (alias: items)
/pubsub create <node> [access]      create a node (models as with 'access')
/pubsub cfg <node>                  node settings (alias: nodecfg)
/pubsub access <node> <open|presence|whitelist|roster|authorize>   change the access
/pubsub groups <node> [group...]    roster groups for 'roster' (alias: rostergroups)
/pubsub roles <node>                who is what at this node (alias: affiliations)
/pubsub role <node> <jid> <owner|publisher|member|outcast|none>    set a role
/pubsub subscribers <node>          who subscribes to this node (alias: who)
/pubsub kick <node> <jid> [subid]   remove a subscriber (alias: remove)
/pubsub request <node> <jid> <yes|no>   answer a subscription request (alias: authorize)
/pubsub retract <node> <id>         take back a single item (alias: undo)
/pubsub purge <node>                empty a node, keep the subscribers (alias: empty)
/pubsub delete <node>               delete a node
```

Without a `<jid>` the request goes to `pubsub.<domain>`; a PEP node belongs to
an account, and then its bare JID stands there. **Every one of these commands
reports what the service answered** — "Subscribed" means it confirmed, not that
it was asked.

The `[subid]` on unsubscribing is needed as soon as there are several
subscriptions to the same node: without it there is no saying which one is
meant, and the client picks none. `/pubsub subs` shows them. With `kick` the
`[subid]` is optional by contrast — without it **all** subscriptions of that JID
go.

`subs` and `subscribers` ask in opposite directions: the first where this client
subscribes, the second who subscribes to a node of your own.

### Encryption (XEP-0384)

```
/omemo on                       switch OMEMO on for this account
/omemo on <jid> <text>          send an encrypted message
/omemo fingerprints             own and known fingerprints (alias: fp)
/omemo trust <jid> <device>     confirm a device
/omemo distrust <jid> <device>  refuse a device
```

### Connection

```
/ping [jid]               send a ping and measure the RTT (XEP-0199)
/keepalive [on|off|sec]   show/change the keepalive
/sm [on|off]              show/change stream management (XEP-0198)
/csi [active|inactive]    client state indication (XEP-0352)
/who                      your own connection status
/carbons                  the carbon status
/reconnect                connect again
/disconnect               disconnect
/raw                      toggle the raw XML display
/help                     help (aliases: /h, /?)
/quit                     quit (aliases: /q, /exit)
```

## How the connection is made

If `-w` gives an endpoint, that endpoint is used and never overruled.
Otherwise the client looks for one (XEP-0156):

1. `https://<domain>/.well-known/host-meta.json`
2. `https://<domain>/.well-known/host-meta`
3. If neither yields a `wss://` endpoint: `wss://<domain>:5443/ws`

Only HTTPS is used for the lookup, and only `wss://` endpoints are taken over —
a BOSH endpoint is read and passed over, this client does not speak it. The
fallback port is ejabberd's default; Prosody usually wants
`wss://<host>:5281/xmpp-websocket`, which you then have to give with `-w`.

`ConnectAsync` **throws** when the setup fails, and every reading step of the
negotiation has a 10 second deadline — the one case an error does not cover is
a far side that accepts the connection and then keeps quiet. After the
connection is up, a break is answered with automatic reconnection: exponential
backoff from 1 second up to 30, at most 5 attempts. The resource is
`console-<pid>` unless the server is left to choose.

## Keepalive

Default interval: **25 seconds**. If stream management is active an `<r/>` is
sent, otherwise an XEP-0199 ping.

```
/keepalive
Keepalive status:
  Enabled: True
  Interval: 25s
  Method: Stream Management <r/>

/keepalive 60      # set the interval to 60s
/keepalive off     # disable
```

Changes take effect only after a reconnect, because the loop is started when
the connection is set up. The same holds for `/sm on|off`.

## End-to-end encryption (OMEMO)

`/omemo on` creates or loads a device identity. The store lies next to the
executable and carries the JID in its name (`omemo-user_example.com.json`), so
two accounts on the same machine are two devices and do not share a
fingerprint.

**The store file is not encrypted.** Whoever can read it can read along.

`/omemo fingerprints` shows your own fingerprint and every device known so far,
grouped in blocks of eight so a human can compare them. Compare a fingerprint
over another channel, never over the one you are securing. `/omemo trust` and
`/omemo distrust` record the decision; devices you have not decided on stay
marked `?`.

When an encrypted message cannot reach a device, the console names it and says
why — a sender who does not learn of it takes their conversation for held.

## Security notes

- **`-p` on the command line is visible.** It lands in the shell history and in
  the process list of the machine. The interactive password prompt does not.
- **The OMEMO store is plaintext.** See above.
- **`/raw` prints everything**, message bodies included. Do not paste that
  output into a bug report unedited.
- `XMPPConsole/Properties/launchSettings.json` is a local debugging profile. If
  you put real credentials in it, keep it out of the repository.

## Architecture

Three layers, cleanly separated:

| Layer | Type | Task |
|---|---|---|
| UI | `Program` | Command line, command dispatch, presentation. Holds no protocol logic |
| Application | `XMPPClient` | Session state (conversation partner, pending contact requests, last message id) and composite operations |
| Protocol | `XMPPConnection` | WebSocket I/O, SASL, resource binding, stanza routing |

Only the first of these lives in this repository; the other two come from
Ratatoskr. `XMPPClient` and `XMPPConnection` write nothing to the console —
everything runs over events and the injected `ILoggerFactory`.

That injected logger is why `ConsoleUI/` exists. A console that keeps an input
line has a problem a pure output does not: the user is typing right now. An
`AddSimpleConsole` writes whenever it suits it, straight into a half-finished
line. `ConsoleOutput` is the one door everything goes through — incoming
events, system notices and the log — under one lock: the input line gives way,
the output appears, the prompt is back afterwards.

Log levels: `Information` for connection steps, `Debug` for protocol details,
`Trace` for single stanzas (that is what `-v` turns on), `Warning` for spoofing
attempts fended off and protocol oddities.

## Repository layout

```
XMPPConsole/                             the console application
├── XMPPConsole.csproj
├── Program.cs                           command line, dispatch, presentation
└── ConsoleUI/
    ├── ConsoleOutput.cs                 one lock, one line, one output
    └── ConsoleOutputLoggerProvider.cs   the log through the same door

XMPPConsole.Tests/                       eight tests on one question:
└── ConsoleOutputTests.cs                does the input line survive output?

docs/STANDARDS.md                        catalogue of relevant RFCs and XEPs
```

Beside this repository, not inside it:

```
../Ratatoskr/                            XMPP protocol: client, server, XEPs, OMEMO
../Hermod/                               network stack: TCP/TLS, HTTP, DNS/SRV, WebSockets
../Styx/                                 data flow / pipeline abstractions
```

| Sibling | Repository | Purpose |
|---|---|---|
| `../Ratatoskr` | [Vanaheimr/Ratatoskr](https://github.com/Vanaheimr/Ratatoskr) | The XMPP protocol — everything this console drives |
| `../Hermod` | [Vanaheimr/Hermod](https://github.com/Vanaheimr/Hermod) | Network stack Ratatoskr builds on |
| `../Styx` | [Vanaheimr/Styx](https://github.com/Vanaheimr/Styx) | Data-flow abstractions Hermod builds on |

The dependency chain runs `XMPPConsole → Ratatoskr → Hermod → Styx`; only the
first reference is declared here, the rest follow.

The namespace of this application is `org.GraphDefined.Vanaheimr.XMPPConsole`
(and `…XMPPConsole.ConsoleUI`); the protocol types come from
`org.GraphDefined.Vanaheimr.Ratatoskr`.

## Tests

```bash
dotnet test XMPPConsole.Tests/XMPPConsole.Tests.csproj
```

Eight tests, and they all ask the same question: does the input line survive an
output that comes unasked? That is the one thing in this repository that can be
got wrong invisibly — a log line written past the prompt looks fine in isolation
and only shows up while somebody is typing. NUnit, in the same versions as the
other Vanaheimr suites.

The protocol is not checked here. Its suite lives with
[Ratatoskr](https://github.com/Vanaheimr/Ratatoskr), in the sibling checkout:

```bash
dotnet test ../Ratatoskr/RatatoskrTests/RatatoskrTests.csproj
```

And everything that needs a foreign implementation — Prosody, ejabberd,
python-omemo — lives in
[XMPPConformanceTests](https://github.com/Vanaheimr/XMPPConformanceTests),
next to the setups that produce those far sides.

## License

Apache License 2.0 — see [LICENSE](LICENSE), like the other
[Vanaheimr](https://github.com/Vanaheimr) projects.
