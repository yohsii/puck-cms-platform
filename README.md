NOTE: this is an asp.net core migration of the mvc 5 project which you can find [here](https://github.com/yohsii/puck). commits were not carried over to this new repo.

# Microsoft New Orchard (formally Puck CMS)
Fast, scalable, code-first, unobtrusive and extensible with powerful querying and Lucene integration.

[![Build status](https://ci.appveyor.com/api/projects/status/7d984nlou8fxw0hq?svg=true)](https://ci.appveyor.com/project/yohsii/puck-core)

[Wiki](https://github.com/yohsii/puck-core/wiki)

**Why use New Orchard**

There are no unnecessary abstractions, if you're already using asp.net mvc then you'll know how to use Puck. Your pages are based on [ViewModels](https://github.com/yohsii/puck-core/wiki/Creating-ViewModels) which are just classes optionally decorated with attributes and the edit screen is made up of [Editor Templates](https://github.com/yohsii/puck-core/wiki/Editor-templates) just like in standard .net mvc. Your razor views (unless you're using Puck [headless](https://github.com/yohsii/puck-core/wiki/Working-with-a-Headless-approach)) will receive instances of the appropriate ViewModel as its Model property and you [query](https://github.com/yohsii/puck-core/wiki/Querying-for-content) for content in a strongly typed manner using the properties of your ViewModels and query expressions.

It's fast, with queries avoiding the database and instead using Lucene. It's also scalable, syncing between servers in a [load balanced](https://github.com/yohsii/puck-core/wiki/Load-Balancing) environment.

**Features**

- Can be used [headless](https://github.com/yohsii/puck-core/wiki/Working-with-a-Headless-approach) or decoupled, as an integrated CMS or a searchable datastore for your apps
- [Live Preview / Visual Editor](https://github.com/yohsii/puck-core/wiki/Live-Preview,-Visual-Editor)
- Multi-site - multiple site roots mapped to different domains
- [Multilingual](https://github.com/yohsii/puck-core/wiki/Multilingual-support) - associate languages with nodes so you can for example, have different site roots with different associated languages. this is recursive so you can associate different languages to nodes further down the hierarchy if necessary. Each content node may also have translations, opening up the possibility for 1:1 and multi-site approaches to multilingual websites.
- Customizable and flexible [workflow system](https://github.com/yohsii/puck-core/wiki/Custom-Workflows) helping you keep track of what needs to be done next
- Strongly typed design - [data querying](https://github.com/yohsii/puck-core/wiki/Querying-for-content) is done in a strongly typed manner, making use of query expressions and a fluent api. templates are also strongly typed.
- Not much to learn - models designed as regular poco decorated with attributes as you normally would with .net mvc
- [Full text search](https://github.com/yohsii/puck-core/wiki/Querying-for-content) - data storage is lucene based and you can set analyzers and field settings (analyze,store,ignore,keep casing) per property in your model
- [Spatial search](https://github.com/yohsii/puck-core/wiki/Querying-for-content#geo-queries)
- [Image cropping](https://www.youtube.com/watch?v=jlPDws8L_FE&t=1s)
- User permissions to grant/deny permissions to particular actions and limit access to content based on start paths
- Hooks - you can [transform](https://github.com/yohsii/puck-core/wiki/Handling-Images-and-Files-with-property-Transformers) data before it is indexed using attributes to modify how a field is indexed and how it is stored
- supports [conditional template switching](https://github.com/yohsii/puck-core/wiki/Display-Modes) (display modes were removed from asp.net core but this works in the same way)
- Redirects - you can manage both 301/302 redirect mappings
- Works in [load balanced](https://github.com/yohsii/puck-core/wiki/Load-Balancing) environments
- Caching - customisable output caching. (per node-type or catch-all. also supports explicit exclusion for any particular node)
- Streamlined pipeline - data retrieval is fast
- Media - media is handled just like any other content, you can expose a IFormFile property in any of your models and it will be properly bound. You can then use data [transformer attributes](https://github.com/yohsii/puck-core/wiki/Handling-Images-and-Files-with-property-Transformers) to decide what should happen to that file before indexing takes place. There are two included transformers which store your images on the local file system or upload them to azure blob storage
- [Task api](https://github.com/yohsii/puck-core/wiki/Background-tasks) - supports one-off and recurring background custom tasks with editable parameters
- Scheduled publish
- [Sync](https://github.com/yohsii/puck-core/wiki/Syncing-content-between-different-databases) content between databases (e.g. staging to production)
- [References](https://github.com/yohsii/puck-core/wiki/Keeping-references-between-pages-to-track-dependencies-of-content) between related content are kept, so you have an idea about which pages or images are dependant on others
- Supports sql server, sqlite, mysql, postgresql

