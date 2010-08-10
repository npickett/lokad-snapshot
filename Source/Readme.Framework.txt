== Snapshot Framework ==

* Snapshot/Restore domain logic for Azure blobs and tables
* Simple classic toolkit approach
* Supports Lokad.Cloud-hosted scenario by splitting long running tasks to shorter ones (-> timeouts)

Flow:
* Distributed on container level (not needed, but side effect of container-oriented toolkit)
* Sequential on segment level (continuations)
* Parallel inside of a segment (if possible)