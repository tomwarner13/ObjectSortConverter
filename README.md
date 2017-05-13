# ObjectSortConverter
A JSON Converter which sorts objects/collections while converting

This is useful when writing large/complex objects to files; using this converter, you can then create a checksum of the created file. If the data changes, the checksum will change. For large amounts of data, we found this easier than comparing 2 large objects in memory.

*Warning:* this code is slow, inefficient, and somewhat brittle. If it is unable to sort collections (for example, if you have a list of objects which do not implement IComparable) it will throw.
