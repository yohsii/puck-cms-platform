NOTE: this is an asp.net core migration of the mvc 5 project which you can find [here](https://github.com/yohsii/puck). commits were not carried over to this new repo.

# puck cms
Fast, scalable, code-first, unobtrusive and extensible with powerful querying and Lucene integration.

[![Build status](https://ci.appveyor.com/api/projects/status/7d984nlou8fxw0hq?svg=true)](https://ci.appveyor.com/project/yohsii/puck-core)

[Wiki](https://github.com/yohsii/puck-core/wiki)

**why use puck**

there are no unnecessary abstractions, if you're already using asp.net mvc then you'll know how to use puck. your pages are based on [ViewModels](https://github.com/yohsii/puck-core/wiki/Creating-ViewModels) which are just classes optionally decorated with attributes and the edit screen is made up of [Editor Templates](https://github.com/yohsii/puck-core/wiki/Editor-templates) just like in standard .net mvc. your razor views will receive instances of the appropriate ViewModel as its Model property and you [query](https://github.com/yohsii/puck-core/wiki/Querying-for-content) for content in a strongly typed manner using the properties of your ViewModels and query expressions.

it's fast, with queries avoiding the database and instead using Lucene. it's also scalable, syncing between servers in a load balanced environment.

**features**

- can be used [headless](https://github.com/yohsii/puck-core/wiki/Working-with-a-Headless-approach) or decoupled
- multi-site - multiple site roots mapped to different domains
- [multilingual](https://github.com/yohsii/puck-core/wiki/Multilingual-support) - associate languages with nodes so you can for example, have different site roots with different associated languages. this is recursive so you can associate different languages to nodes further down the hierarchy if necessary. each content node may also have translations, opening up the possibility for 1:1 and multi-site approaches to multilingual websites.
- strongly typed design - [data querying](https://github.com/yohsii/puck-core/wiki/Querying-for-content) is done in a strongly typed manner, making use of query expressions and a fluent api. templates are also strongly typed.
- not much to learn - models designed as regular poco decorated with attributes as you normally would with .net mvc
- [full text search](https://github.com/yohsii/puck-core/wiki/Querying-for-content) - data storage is lucene based and you can set analyzers and field settings (analyze,store,ignore,keep casing) per property in your model
- [spatial search](https://github.com/yohsii/puck-core/wiki/Querying-for-content)
- [image cropping](https://www.youtube.com/watch?v=jlPDws8L_FE&t=1s)
- basic user permissions to grant/deny permissions to particular actions and limit access to content based on a start path
- hooks - you can [transform](https://github.com/yohsii/puck-core/wiki/Handling-Images-and-Files-with-property-Transformers) data before it is indexed using attributes to modify how a field is indexed and how it is stored
- supports conditional template switching (display modes were removed from asp.net core but this works in the same way)
- redirects - you can manage both 301/302 redirect mappings
- works in [load balanced](https://github.com/yohsii/puck-core/wiki/Load-Balancing) environments
- caching - customisable output caching. (per node-type or catch-all. also supports explicit exclusion for any particular node)
- streamlined pipeline - data retrieval is fast
- media - media is handled just like any other content, you can expose a IFormFile property in any of your models and it will be properly bound. you can then use data [transformer attributes](https://github.com/yohsii/puck-core/wiki/Handling-Images-and-Files-with-property-Transformers) to decide what should happen to that file before indexing takes place. there are two included transformers which store your images on the local file system or upload them to azure blob storage
- task api - supports one-off and recurring background custom tasks with editable parameters
- scheduled publish
- [sync](https://github.com/yohsii/puck-core/wiki/Syncing-content-between-different-databases) content between databases (e.g. staging to production)
- supports sql server, sqlite, mysql, postgresql
