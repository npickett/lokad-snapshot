== Cloud Host ==

* tiny custom Lokad.Cloud runtime for hosting a set of queue cloud worker services
* RSM-based reporting
* automatically taking (and pruning) snapshots of a set of configurable accounts
* thin MVC web role to manually create snapshots or queue a restore
* heavily-simplified pragmatic CQRS approach

Commands:
* async, first class
* no differentiation between commands and internal events (no need for internal events)
* command handlers = cloud queue services (no need for message bus)
* invoked by activators or manually through web role

Reporting:
* persisted reports, slightly denormalized
* sync publisher (no need for first class domain events)
* stale data (only updated from queued workers)

Limitations:
* maximum account name length: 18 characters (instead of 24 as allowed by the Azure portal)