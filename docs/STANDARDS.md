# Standards catalogue (roadmap)

> This is the **target scope**, not the implementation status. It catalogues the
> specifications that are relevant for a modern XMPP client so that decisions
> about what to build next have a reference. What XMPPConsole actually speaks
> today is listed in the [README](../README.md#what-it-speaks-today); what the
> protocol library underneath implements is listed in the
> [Ratatoskr README](../libs/Ratatoskr/README.md).

---

The overview below lists every specification relevant to a modern XMPP client.
The status of each XEP is a snapshot (as of August 2026); what holds is always
the [XEP list of the XSF](https://xmpp.org/extensions/).

The XSF status vocabulary: `Final` › `Draft`/`Stable` › `Proposed` ›
`Experimental` › `Deferred` (≙ twelve months without activity), plus
`Active`/`Historical` for documents outside the standards track, and
`Deprecated` and `Obsolete`.

### RFCs

#### Current

| RFC | Title | What it means for the client |
|---|---|---|
| [RFC 6120](https://www.rfc-editor.org/rfc/rfc6120) | XMPP: Core | XML streams, TLS, SASL, resource binding, stanzas, error handling |
| [RFC 6121](https://www.rfc-editor.org/rfc/rfc6121) | XMPP: Instant Messaging and Presence | Roster, subscriptions, presence, `<message/>` semantics |
| [RFC 7622](https://www.rfc-editor.org/rfc/rfc7622) | XMPP: Address Format | JID syntax and normalisation (PRECIS); supersedes RFC 6122 |
| [RFC 7590](https://www.rfc-editor.org/rfc/rfc7590) | Use of TLS in XMPP | TLS required, minimum requirements; updates RFC 6120 |
| [RFC 7395](https://www.rfc-editor.org/rfc/rfc7395) | XMPP Subprotocol for WebSocket | XMPP over WebSocket (`wss://`) |
| [RFC 7712](https://www.rfc-editor.org/rfc/rfc7712) | Domain Name Associations (DNA) | POSH, DANE binding of domain to certificate |
| [RFC 5122](https://www.rfc-editor.org/rfc/rfc5122) | IRI/URI Scheme for XMPP | `xmpp:` URIs (deep links, `?message`, `?join`) |
| [RFC 4854](https://www.rfc-editor.org/rfc/rfc4854) | URN Sub-Namespace for XMPP | `urn:xmpp:*` namespaces |
| [RFC 2782](https://www.rfc-editor.org/rfc/rfc2782) | DNS SRV RR | `_xmpp-client._tcp` resolution |
| [RFC 9525](https://www.rfc-editor.org/rfc/rfc9525) | Service Identity in TLS | Certificate checking; supersedes RFC 6125 |
| [RFC 4422](https://www.rfc-editor.org/rfc/rfc4422) | SASL | Authentication framework |
| [RFC 5802](https://www.rfc-editor.org/rfc/rfc5802) | SCRAM-SHA-1 | Mandatory mechanism per RFC 6120 |
| [RFC 7677](https://www.rfc-editor.org/rfc/rfc7677) | SCRAM-SHA-256 | The SCRAM mechanism preferred today |
| [RFC 5801](https://www.rfc-editor.org/rfc/rfc5801) | GS2 / SASL Channel Binding | The `-PLUS` variants of the SCRAM mechanisms |
| [RFC 5929](https://www.rfc-editor.org/rfc/rfc5929) | Channel Bindings for TLS | `tls-unique`, `tls-server-end-point` (up to TLS 1.2) |
| [RFC 9266](https://www.rfc-editor.org/rfc/rfc9266) | Channel Bindings for TLS 1.3 | `tls-exporter` — required for TLS 1.3 |
| [RFC 6749](https://www.rfc-editor.org/rfc/rfc6749) / [RFC 7628](https://www.rfc-editor.org/rfc/rfc7628) | OAuth 2.0 / SASL-OAUTH | Token-based login (cf. XEP-0493) |
| [RFC 8305](https://www.rfc-editor.org/rfc/rfc8305) | Happy Eyeballs v2 | Parallel IPv4/IPv6 connection setup (cf. XEP-0495) |
| [RFC 7247](https://www.rfc-editor.org/rfc/rfc7247) | SIP–XMPP Interworking: Architecture | Gateway scenarios |
| [RFC 7572](https://www.rfc-editor.org/rfc/rfc7572) | SIP–XMPP: Instant Messaging | Gateway scenarios |
| [RFC 7573](https://www.rfc-editor.org/rfc/rfc7573) | SIP–XMPP: One-to-One Text Chat | Gateway scenarios |
| [RFC 7702](https://www.rfc-editor.org/rfc/rfc7702) | SIP–XMPP: Groupchat | Gateway scenarios |

#### Superseded / historical (do not implement)

| RFC | Title | Note |
|---|---|---|
| [RFC 3920](https://www.rfc-editor.org/rfc/rfc3920) | XMPP: Core (2004) | **obsolete** — replaced by RFC 6120 |
| [RFC 3921](https://www.rfc-editor.org/rfc/rfc3921) | XMPP: IM and Presence (2004) | **obsolete** — replaced by RFC 6121 (including the `<session/>` bind, which is gone) |
| [RFC 3922](https://www.rfc-editor.org/rfc/rfc3922) | Mapping XMPP to CPIM | **obsolete** |
| [RFC 3923](https://www.rfc-editor.org/rfc/rfc3923) | E2E Signing and Object Encryption | **obsolete** — never implemented to any notable degree; today OMEMO/OX |
| [RFC 4622](https://www.rfc-editor.org/rfc/rfc4622) | IRI/URI Scheme for XMPP | **obsolete** — replaced by RFC 5122 |
| [RFC 6122](https://www.rfc-editor.org/rfc/rfc6122) | XMPP Address Format (Nodeprep/Resourceprep) | **obsolete** — replaced by RFC 7622 |
| [RFC 6125](https://www.rfc-editor.org/rfc/rfc6125) | Service Identity in TLS | **obsolete** — replaced by RFC 9525 |

> Note: the `<session/>` feature from RFC 3921 §3 is still offered by some
> servers; RFC 6121 struck it without replacement, and a modern client ignores
> it.

### XEPs — core and infrastructure

| XEP | Title | Status | Note |
|---|---|---|---|
| [XEP-0030](https://xmpp.org/extensions/xep-0030.html) | Service Discovery | Final | The basis of very nearly every feature negotiation |
| [XEP-0115](https://xmpp.org/extensions/xep-0115.html) | Entity Capabilities | Stable | Caps hashing; required for "Core Client" |
| [XEP-0390](https://xmpp.org/extensions/xep-0390.html) | Entity Capabilities 2.0 | Experimental | Successor to XEP-0115 |
| [XEP-0128](https://xmpp.org/extensions/xep-0128.html) | Service Discovery Extensions | Active | Data forms in disco answers |
| [XEP-0004](https://xmpp.org/extensions/xep-0004.html) | Data Forms | Final | Forms in registration, MUC config, ad-hoc |
| [XEP-0122](https://xmpp.org/extensions/xep-0122.html) | Data Forms Validation | Stable | |
| [XEP-0050](https://xmpp.org/extensions/xep-0050.html) | Ad-Hoc Commands | Stable | Server and component control — of particular interest to a CLI |
| [XEP-0059](https://xmpp.org/extensions/xep-0059.html) | Result Set Management | Stable | Pagination (MAM, disco, PubSub) |
| [XEP-0060](https://xmpp.org/extensions/xep-0060.html) | Publish-Subscribe | Stable | |
| [XEP-0163](https://xmpp.org/extensions/xep-0163.html) | Personal Eventing Protocol (PEP) | Stable | Avatars, bookmarks, OMEMO bundles, OX keys |
| [XEP-0223](https://xmpp.org/extensions/xep-0223.html) | Persistent Storage of Private Data via PubSub | Active | Private client storage |
| [XEP-0222](https://xmpp.org/extensions/xep-0222.html) | Persistent Storage of Public Data via PubSub | Active | |
| [XEP-0082](https://xmpp.org/extensions/xep-0082.html) | XMPP Date and Time Profiles | Active | |
| [XEP-0106](https://xmpp.org/extensions/xep-0106.html) | JID Escaping | Stable | Gateway JIDs, special characters |
| [XEP-0203](https://xmpp.org/extensions/xep-0203.html) | Delayed Delivery | Final | Timestamps for offline and archived messages |
| [XEP-0297](https://xmpp.org/extensions/xep-0297.html) | Stanza Forwarding | Final | The basis of carbons and MAM |
| [XEP-0359](https://xmpp.org/extensions/xep-0359.html) | Unique and Stable Stanza IDs | Experimental | In use everywhere in practice (corrections, reactions, MAM) |
| [XEP-0334](https://xmpp.org/extensions/xep-0334.html) | Message Processing Hints | Stable | `no-store`, `no-copy`, `store` |
| [XEP-0372](https://xmpp.org/extensions/xep-0372.html) | References | Experimental | Mentions, quotations, embedded media |
| [XEP-0199](https://xmpp.org/extensions/xep-0199.html) | XMPP Ping | Final | Keepalive / liveness |
| [XEP-0231](https://xmpp.org/extensions/xep-0231.html) | Bits of Binary | Stable | Inline data (avatars, CAPTCHAs) |
| [XEP-0114](https://xmpp.org/extensions/xep-0114.html) | Jabber Component Protocol | Active | Only relevant if the CLI is to run as a component |

### XEPs — connection, authentication, mobile

| XEP | Title | Status | Note |
|---|---|---|---|
| [XEP-0368](https://xmpp.org/extensions/xep-0368.html) | SRV records for XMPP over TLS | Stable | Direct TLS (`_xmpps-client._tcp`) |
| [XEP-0156](https://xmpp.org/extensions/xep-0156.html) | Discovering Alternative XMPP Connection Methods | Stable | `host-meta` / `.well-known` for WebSocket and BOSH |
| [XEP-0487](https://xmpp.org/extensions/xep-0487.html) | Host Meta 2 | Experimental | Unified connection discovery |
| [XEP-0495](https://xmpp.org/extensions/xep-0495.html) | Happy Eyeballs | Experimental | Connection strategy across several endpoints |
| [XEP-0124](https://xmpp.org/extensions/xep-0124.html) | BOSH | Stable | HTTP long-polling transport |
| [XEP-0206](https://xmpp.org/extensions/xep-0206.html) | XMPP Over BOSH | Stable | |
| [XEP-0198](https://xmpp.org/extensions/xep-0198.html) | Stream Management | Stable | Acks and resumption — required in all but name |
| [XEP-0388](https://xmpp.org/extensions/xep-0388.html) | Extensible SASL Profile (SASL2) | Stable | The modern login handshake |
| [XEP-0386](https://xmpp.org/extensions/xep-0386.html) | Bind 2 | Stable | Bind, SM and carbons in one round trip |
| [XEP-0484](https://xmpp.org/extensions/xep-0484.html) | Fast Authentication Streamlining Tokens (FAST) | Proposed | Token login without a password in memory |
| [XEP-0440](https://xmpp.org/extensions/xep-0440.html) | SASL Channel-Binding Type Capability | Stable | Negotiating `tls-exporter` and the like |
| [XEP-0474](https://xmpp.org/extensions/xep-0474.html) | SASL SCRAM Downgrade Protection | Experimental | |
| [XEP-0480](https://xmpp.org/extensions/xep-0480.html) | SASL Upgrade Tasks | Experimental | Migration to stronger SCRAM hashes |
| [XEP-0257](https://xmpp.org/extensions/xep-0257.html) | Client Certificate Management for SASL EXTERNAL | Deferred | Client certificate login |
| [XEP-0493](https://xmpp.org/extensions/xep-0493.html) | OAuth Client Login | Experimental | |
| [XEP-0494](https://xmpp.org/extensions/xep-0494.html) | Client Access Management | Experimental | Session and device management |
| [XEP-0352](https://xmpp.org/extensions/xep-0352.html) | Client State Indication | Stable | `active`/`inactive` — saving traffic |
| [XEP-0357](https://xmpp.org/extensions/xep-0357.html) | Push Notifications | Deferred | Widely deployed in spite of the status |
| [XEP-0286](https://xmpp.org/extensions/xep-0286.html) | XMPP on Mobile Devices | Stable | Informational: best practices |
| [XEP-0077](https://xmpp.org/extensions/xep-0077.html) | In-Band Registration | Final | Creating and deleting accounts, changing passwords |
| [XEP-0401](https://xmpp.org/extensions/xep-0401.html) | Easy User Onboarding | Experimental | Invitation links |
| [XEP-0379](https://xmpp.org/extensions/xep-0379.html) | Pre-Authenticated Roster Subscription | Proposed | |

### XEPs — instant messaging

| XEP | Title | Status | Note |
|---|---|---|---|
| [XEP-0280](https://xmpp.org/extensions/xep-0280.html) | Message Carbons | Stable | Multi-device synchronisation |
| [XEP-0313](https://xmpp.org/extensions/xep-0313.html) | Message Archive Management (MAM) | Stable | Server-side history |
| [XEP-0085](https://xmpp.org/extensions/xep-0085.html) | Chat State Notifications | Final | "is typing …" |
| [XEP-0184](https://xmpp.org/extensions/xep-0184.html) | Message Delivery Receipts | Stable | Delivery confirmation |
| [XEP-0333](https://xmpp.org/extensions/xep-0333.html) | Chat Markers | Stable | Received / displayed |
| [XEP-0490](https://xmpp.org/extensions/xep-0490.html) | Message Displayed Synchronization | Stable | Read state across devices |
| [XEP-0308](https://xmpp.org/extensions/xep-0308.html) | Last Message Correction | Stable | Correcting a message after the fact |
| [XEP-0424](https://xmpp.org/extensions/xep-0424.html) | Message Retraction | Proposed | Taking a message back |
| [XEP-0444](https://xmpp.org/extensions/xep-0444.html) | Message Reactions | Experimental | Emoji reactions |
| [XEP-0461](https://xmpp.org/extensions/xep-0461.html) | Message Replies | Experimental | Reply references / threads |
| [XEP-0428](https://xmpp.org/extensions/xep-0428.html) | Fallback Indication | Experimental | Marking fallback text (E2EE, replies) |
| [XEP-0393](https://xmpp.org/extensions/xep-0393.html) | Message Styling | Stable | Lightweight markup — successor to XHTML-IM |
| [XEP-0245](https://xmpp.org/extensions/xep-0245.html) | The /me Command | Active | |
| [XEP-0191](https://xmpp.org/extensions/xep-0191.html) | Blocking Command | Stable | Successor to the privacy lists |
| [XEP-0481](https://xmpp.org/extensions/xep-0481.html) | Content Types in Messages | Experimental | Markdown markup, for instance |
| [XEP-0492](https://xmpp.org/extensions/xep-0492.html) | Chat Notification Settings | Experimental | Synchronising mute settings |
| [XEP-0392](https://xmpp.org/extensions/xep-0392.html) | Consistent Color Generation | Stable | Uniform nick colours (useful in a terminal too) |

### XEPs — group chat

| XEP | Title | Status | Note |
|---|---|---|---|
| [XEP-0045](https://xmpp.org/extensions/xep-0045.html) | Multi-User Chat (MUC) | Stable | The established group chat |
| [XEP-0249](https://xmpp.org/extensions/xep-0249.html) | Direct MUC Invitations | Stable | |
| [XEP-0410](https://xmpp.org/extensions/xep-0410.html) | MUC Self-Ping | Stable | Detecting a silent disconnect from a room |
| [XEP-0421](https://xmpp.org/extensions/xep-0421.html) | Occupant Id | Stable | Stable participant identity (even anonymously) |
| [XEP-0425](https://xmpp.org/extensions/xep-0425.html) | Message Moderation | Experimental | |
| [XEP-0486](https://xmpp.org/extensions/xep-0486.html) | MUC Avatars | Experimental | |
| [XEP-0488](https://xmpp.org/extensions/xep-0488.html) | MUC Token Invite | Experimental | |
| [XEP-0402](https://xmpp.org/extensions/xep-0402.html) | PEP Native Bookmarks (Bookmarks 2) | Stable | Replaces XEP-0048 |
| [XEP-0369](https://xmpp.org/extensions/xep-0369.html) | MIX-CORE | Experimental | The MUC successor family |
| [XEP-0403](https://xmpp.org/extensions/xep-0403.html) | MIX-PRESENCE | Experimental | |
| [XEP-0404](https://xmpp.org/extensions/xep-0404.html) | MIX-ANON | Experimental | |
| [XEP-0405](https://xmpp.org/extensions/xep-0405.html) | MIX-PAM | Experimental | |
| [XEP-0406](https://xmpp.org/extensions/xep-0406.html) | MIX-ADMIN | Experimental | |
| [XEP-0407](https://xmpp.org/extensions/xep-0407.html) | MIX-MISC | Experimental | |

### XEPs — file transfer and media

| XEP | Title | Status | Note |
|---|---|---|---|
| [XEP-0363](https://xmpp.org/extensions/xep-0363.html) | HTTP File Upload | Stable | The standard way to send a file today |
| [XEP-0066](https://xmpp.org/extensions/xep-0066.html) | Out of Band Data | Stable | `jabber:x:oob` accompanies a 0363 upload and says that the URL is a file. Ratatoskr discards the element, which is why `--storeChatMedia` recognises shared files by the body. The IQ half (`jabber:iq:oob`) is dead in practice but not deprecated — unlike XEP-0095/0096 |
| [XEP-0446](https://xmpp.org/extensions/xep-0446.html) | File Metadata Element | Experimental | |
| [XEP-0447](https://xmpp.org/extensions/xep-0447.html) | Stateless File Sharing (SFS) | Experimental | |
| [XEP-0448](https://xmpp.org/extensions/xep-0448.html) | Encryption for Stateless File Sharing | Experimental | |
| [XEP-0385](https://xmpp.org/extensions/xep-0385.html) | Stateless Inline Media Sharing (SIMS) | Experimental | Predecessor of SFS |
| [XEP-0454](https://xmpp.org/extensions/xep-0454.html) | OMEMO Media Sharing | Experimental | `aesgcm:` URLs |
| [XEP-0300](https://xmpp.org/extensions/xep-0300.html) | Use of Cryptographic Hash Functions | Final | Hashes in file metadata |
| [XEP-0264](https://xmpp.org/extensions/xep-0264.html) | Jingle Content Thumbnails | Experimental | |
| [XEP-0234](https://xmpp.org/extensions/xep-0234.html) | Jingle File Transfer | Deferred | Peer-to-peer transfer |
| [XEP-0260](https://xmpp.org/extensions/xep-0260.html) | Jingle SOCKS5 Bytestreams Transport | Stable | |
| [XEP-0261](https://xmpp.org/extensions/xep-0261.html) | Jingle In-Band Bytestreams Transport | Stable | |
| [XEP-0065](https://xmpp.org/extensions/xep-0065.html) | SOCKS5 Bytestreams | Stable | |
| [XEP-0047](https://xmpp.org/extensions/xep-0047.html) | In-Band Bytestreams | Final | |

### XEPs — Jingle / audio / video

| XEP | Title | Status | Note |
|---|---|---|---|
| [XEP-0166](https://xmpp.org/extensions/xep-0166.html) | Jingle | Stable | Signalling framework |
| [XEP-0167](https://xmpp.org/extensions/xep-0167.html) | Jingle RTP Sessions | Stable | Audio and video |
| [XEP-0176](https://xmpp.org/extensions/xep-0176.html) | Jingle ICE-UDP Transport | Stable | |
| [XEP-0177](https://xmpp.org/extensions/xep-0177.html) | Jingle Raw UDP Transport | Stable | |
| [XEP-0320](https://xmpp.org/extensions/xep-0320.html) | Use of DTLS-SRTP in Jingle Sessions | Stable | Media encryption |
| [XEP-0353](https://xmpp.org/extensions/xep-0353.html) | Jingle Message Initiation | Experimental | Call signalling to every device |
| [XEP-0215](https://xmpp.org/extensions/xep-0215.html) | External Service Discovery | Stable | Finding STUN/TURN servers |
| [XEP-0293](https://xmpp.org/extensions/xep-0293.html) | Jingle RTP Feedback Negotiation | Stable | |
| [XEP-0294](https://xmpp.org/extensions/xep-0294.html) | Jingle RTP Header Extensions | Stable | |
| [XEP-0338](https://xmpp.org/extensions/xep-0338.html) | Jingle Grouping Framework | Stable | BUNDLE |
| [XEP-0339](https://xmpp.org/extensions/xep-0339.html) | Source-Specific Media Attributes | Stable | |
| [XEP-0343](https://xmpp.org/extensions/xep-0343.html) | Signaling WebRTC Data Channels in Jingle | Experimental | |
| [XEP-0482](https://xmpp.org/extensions/xep-0482.html) | Call Invites | Experimental | |
| [XEP-0483](https://xmpp.org/extensions/xep-0483.html) | HTTP Online Meetings | Experimental | |
| [XEP-0298](https://xmpp.org/extensions/xep-0298.html) | Delivering Conference Information (Coin) | Deferred | |

### XEPs — end-to-end encryption

| XEP | Title | Status | Note |
|---|---|---|---|
| [XEP-0384](https://xmpp.org/extensions/xep-0384.html) | OMEMO Encryption | Experimental | The de facto standard; the current version uses `twomemo`/OMEMO 2 |
| [XEP-0420](https://xmpp.org/extensions/xep-0420.html) | Stanza Content Encryption (SCE) | Experimental | The basis of OMEMO 2 and OX |
| [XEP-0373](https://xmpp.org/extensions/xep-0373.html) | OpenPGP for XMPP (OX) | Experimental | |
| [XEP-0374](https://xmpp.org/extensions/xep-0374.html) | OpenPGP for XMPP Instant Messaging | Experimental | |
| [XEP-0434](https://xmpp.org/extensions/xep-0434.html) | Trust Messages | Experimental | Synchronising key trust |
| [XEP-0450](https://xmpp.org/extensions/xep-0450.html) | Automatic Trust Management (ATM) | Experimental | |
| [XEP-0510](https://xmpp.org/extensions/xep-0510.html) | End-to-End Encrypted Contacts Metadata | Experimental | |

### XEPs — user profile, presence, everything else

| XEP | Title | Status | Note |
|---|---|---|---|
| [XEP-0084](https://xmpp.org/extensions/xep-0084.html) | User Avatar | Stable | PEP-based avatars |
| [XEP-0153](https://xmpp.org/extensions/xep-0153.html) | vCard-Based Avatars | Active | Legacy, still needed for interoperability |
| [XEP-0398](https://xmpp.org/extensions/xep-0398.html) | User Avatar to vCard-Based Avatar Conversion | Stable | The bridge between the two worlds |
| [XEP-0054](https://xmpp.org/extensions/xep-0054.html) | vcard-temp | Active | Historical, and in practice indispensable |
| [XEP-0292](https://xmpp.org/extensions/xep-0292.html) | vCard4 Over XMPP | Experimental | The modern successor to XEP-0054 |
| [XEP-0012](https://xmpp.org/extensions/xep-0012.html) | Last Activity | Final | |
| [XEP-0202](https://xmpp.org/extensions/xep-0202.html) | Entity Time | Final | |
| [XEP-0092](https://xmpp.org/extensions/xep-0092.html) | Software Version | Stable | |
| [XEP-0232](https://xmpp.org/extensions/xep-0232.html) | Software Information | Deferred | The disco-based alternative to XEP-0092 |
| [XEP-0157](https://xmpp.org/extensions/xep-0157.html) | Contact Addresses for XMPP Services | Active | Showing abuse and admin contacts |
| [XEP-0489](https://xmpp.org/extensions/xep-0489.html) | Reporting Account Affiliations | Experimental | Spam and abuse reports |
| [XEP-0377](https://xmpp.org/extensions/xep-0377.html) | Spam Reporting | Experimental | |

### XEPs — deprecated / obsolete

The XSF has marked these specifications **Deprecated** (superseded, new
implementations unwanted) or **Obsolete** (withdrawn). For interoperability
with what is already out there, *reading* support for some of them can make
sense — a client should not send them anew.

#### Deprecated

| XEP | Title | Replacement |
|---|---|---|
| [XEP-0013](https://xmpp.org/extensions/xep-0013.html) | Flexible Offline Message Retrieval | MAM (XEP-0313) |
| [XEP-0016](https://xmpp.org/extensions/xep-0016.html) | Privacy Lists | Blocking Command (XEP-0191) |
| [XEP-0020](https://xmpp.org/extensions/xep-0020.html) | Feature Negotiation | Data Forms / disco |
| [XEP-0048](https://xmpp.org/extensions/xep-0048.html) | Bookmarks | PEP Native Bookmarks (XEP-0402) |
| [XEP-0071](https://xmpp.org/extensions/xep-0071.html) | XHTML-IM | Message Styling (XEP-0393) |
| [XEP-0086](https://xmpp.org/extensions/xep-0086.html) | Error Condition Mappings | The error syntax of RFC 6120 |
| [XEP-0093](https://xmpp.org/extensions/xep-0093.html) | Roster Item Exchange | XEP-0144 |
| [XEP-0095](https://xmpp.org/extensions/xep-0095.html) | Stream Initiation | Jingle (XEP-0166/0234) |
| [XEP-0096](https://xmpp.org/extensions/xep-0096.html) | SI File Transfer | Jingle File Transfer (XEP-0234), HTTP Upload (XEP-0363) |
| [XEP-0126](https://xmpp.org/extensions/xep-0126.html) | Invisibility | — |
| [XEP-0130](https://xmpp.org/extensions/xep-0130.html) | Waiting Lists | — |
| [XEP-0136](https://xmpp.org/extensions/xep-0136.html) | Message Archiving | MAM (XEP-0313) |
| [XEP-0137](https://xmpp.org/extensions/xep-0137.html) | Publishing Stream Initiation Requests | — |
| [XEP-0256](https://xmpp.org/extensions/xep-0256.html) | Last Activity in Presence | — |
| [XEP-0411](https://xmpp.org/extensions/xep-0411.html) | Bookmarks Conversion | Purely a migration aid, 0048 → 0402 |

#### Obsolete

| XEP | Title | Note |
|---|---|---|
| [XEP-0003](https://xmpp.org/extensions/xep-0003.html) | Proxy Accept Socket Service (PASS) | |
| [XEP-0005](https://xmpp.org/extensions/xep-0005.html) | Jabber Interest Groups | |
| [XEP-0008](https://xmpp.org/extensions/xep-0008.html) | IQ-Based Avatars | Replaced by XEP-0084/0153 |
| [XEP-0011](https://xmpp.org/extensions/xep-0011.html) | Jabber Browsing | Replaced by Service Discovery (XEP-0030) |
| [XEP-0022](https://xmpp.org/extensions/xep-0022.html) | Message Events | Replaced by XEP-0085 and XEP-0184 |
| [XEP-0023](https://xmpp.org/extensions/xep-0023.html) | Message Expiration | |
| [XEP-0025](https://xmpp.org/extensions/xep-0025.html) | Jabber HTTP Polling | Replaced by BOSH / WebSocket |
| [XEP-0027](https://xmpp.org/extensions/xep-0027.html) | Current Jabber OpenPGP Usage | Replaced by OX (XEP-0373/0374) |
| [XEP-0038](https://xmpp.org/extensions/xep-0038.html) | Icon Styles | |
| [XEP-0051](https://xmpp.org/extensions/xep-0051.html) | Connection Transfer | |
| [XEP-0073](https://xmpp.org/extensions/xep-0073.html) | Basic IM Protocol Suite | Replaced by the compliance suites |
| [XEP-0078](https://xmpp.org/extensions/xep-0078.html) | Non-SASL Authentication | Replaced by SASL (RFC 6120) |
| [XEP-0090](https://xmpp.org/extensions/xep-0090.html) | Legacy Entity Time | Replaced by XEP-0202 |
| [XEP-0091](https://xmpp.org/extensions/xep-0091.html) | Legacy Delayed Delivery | Replaced by XEP-0203 |
| [XEP-0094](https://xmpp.org/extensions/xep-0094.html) | Agent Information | |
| [XEP-0112](https://xmpp.org/extensions/xep-0112.html) | User Physical Location | Replaced by XEP-0080 |
| [XEP-0117](https://xmpp.org/extensions/xep-0117.html) | Intermediate IM Protocol Suite | |
| [XEP-0138](https://xmpp.org/extensions/xep-0138.html) | Stream Compression | Security risks; avoid TLS compression as well |
| [XEP-0146](https://xmpp.org/extensions/xep-0146.html) | Remote Controlling Clients | |
| [XEP-0190](https://xmpp.org/extensions/xep-0190.html) | Best Practice for Closing Idle Streams | |
| [XEP-0192](https://xmpp.org/extensions/xep-0192.html) | Proposed Stream Feature Improvements | |
| [XEP-0193](https://xmpp.org/extensions/xep-0193.html) | Proposed Resource Binding Improvements | |
| [XEP-0211](https://xmpp.org/extensions/xep-0211.html) | XMPP Basic Client 2008 | Compliance suite (outdated) |
| [XEP-0212](https://xmpp.org/extensions/xep-0212.html) | XMPP Basic Server 2008 | Compliance suite (outdated) |
| [XEP-0213](https://xmpp.org/extensions/xep-0213.html) | XMPP Intermediate IM Client 2008 | Compliance suite (outdated) |
| [XEP-0216](https://xmpp.org/extensions/xep-0216.html) | XMPP Intermediate IM Server 2008 | Compliance suite (outdated) |
| [XEP-0229](https://xmpp.org/extensions/xep-0229.html) | Stream Compression with LZW | |
| [XEP-0237](https://xmpp.org/extensions/xep-0237.html) | Roster Versioning | Absorbed into RFC 6121 |
| [XEP-0242](https://xmpp.org/extensions/xep-0242.html) | XMPP Client Compliance 2009 | |
| [XEP-0243](https://xmpp.org/extensions/xep-0243.html) | XMPP Server Compliance 2009 | |
| [XEP-0270](https://xmpp.org/extensions/xep-0270.html) | XMPP Compliance Suites 2010 | |
| [XEP-0302](https://xmpp.org/extensions/xep-0302.html) | XMPP Compliance Suites 2012 | |
| [XEP-0387](https://xmpp.org/extensions/xep-0387.html) | XMPP Compliance Suites 2018 | |
| [XEP-0412](https://xmpp.org/extensions/xep-0412.html) | XMPP Compliance Suites 2019 | |
| [XEP-0423](https://xmpp.org/extensions/xep-0423.html) | XMPP Compliance Suites 2020 | |
| [XEP-0443](https://xmpp.org/extensions/xep-0443.html) | XMPP Compliance Suites 2021 | |
| [XEP-0459](https://xmpp.org/extensions/xep-0459.html) | XMPP Compliance Suites 2022 | Replaced by XEP-0479 |

> An aside: [XEP-0049](https://xmpp.org/extensions/xep-0049.html) (Private XML
> Storage) is formally `Active` but counts as superseded — modern clients use
> PEP (XEP-0163/0223) rather than `jabber:iq:private`. Likewise
> [XEP-0054](https://xmpp.org/extensions/xep-0054.html) (vcard-temp) and
> [XEP-0153](https://xmpp.org/extensions/xep-0153.html) are historical, and
> still required by the current compliance suite for interoperability.

### Compliance suites

The frame of reference for scope is
**[XEP-0479: XMPP Compliance Suites 2023](https://xmpp.org/extensions/xep-0479.html)**
(Experimental) — the current edition. It defines categories (Core, Web, IM,
Mobile, A/V Calling), each with a *Core* and an *Advanced* level.

The part that concerns a client:

| Category | Core level | Advanced level (in addition) |
|---|---|---|
| **Core** | RFC 6120, RFC 7590, XEP-0030, XEP-0115 | XEP-0368, XEP-0163 |
| **Web** | RFC 7395, XEP-0124, XEP-0206 | XEP-0156 |
| **IM** | RFC 6121, XEP-0045, XEP-0054, XEP-0249, XEP-0280, XEP-0363 | XEP-0048, XEP-0049, XEP-0084, XEP-0085, XEP-0153, XEP-0184, XEP-0191, XEP-0198, XEP-0223, XEP-0234, XEP-0245, XEP-0261, XEP-0308, XEP-0313, XEP-0398, XEP-0402, XEP-0410 |
| **Mobile** | XEP-0198, XEP-0352 | XEP-0357 |
| **A/V Calling** | XEP-0167, XEP-0176, XEP-0215, XEP-0320, XEP-0353 | XEP-0293, XEP-0294, XEP-0338, XEP-0339 |

As a practical reference alongside it: the
[XMPP Compliance Tester](https://compliance.conversations.im/) and the feature
matrix at [xmpp.org/software/clients](https://xmpp.org/software/clients/).
