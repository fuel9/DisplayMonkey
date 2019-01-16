
![DisplayMonkey](http://www.displaymonkey.org/dm/wp-content/uploads/display_monkey_whi_red_cool_6.png)

### Digital Signage Solution for Everybody, Anywhere

[Website](http://displaymonkey.org) |
[Community](http://www.displaymonkey.org/dm/answers/) |
[Documentation](http://www.displaymonkey.org/dm/documentation/) |
[Binaries](http://www.displaymonkey.org/dm/download/) |
[Installation](http://www.displaymonkey.org/dm/documentation/installation/) |
[Twitter](https://twitter.com/fuel9) |
[Linked-in](https://www.linkedin.com/company/fuel9?trk=company_logo)

Display Monkey is an awesome and easy to use browser based digital signage solution that can spread the gospel in stores, hallways, conference rooms and shop floors. 

Display Monkey is written with the following products, frameworks and technologies:
- [ASP.Net MVC](http://www.asp.net/mvc) 5 with C#/Razor
- [Entity Framework](https://msdn.microsoft.com/en-us/data/ef.aspx) 6 with Data-first model context
- [jQuery](http://jquery.com) 2.1 + jQueryUI + plugins, [Prototype](http://prototypejs.org) 1.7, [Script.aculo.us](http://script.aculo.us), [Moment.js](http://momentjs.com), [Modernizr](https://modernizr.com)
- Microsoft SQL Server 2005+
- Microsoft Exchange Web Services
- Google and Yahoo APIs

As of this writing Display Monkey server components run on IIS 7 and the front-end side supports the following browsers:
- Firefox
- Chrome
- Safari
- IE (8 end up)
- Raspberry Pi Epiphany
- Samsung Smart TV browser (6-series and up)

### Why

Display Monkey provides an affordable yet powerful and modern alternative to commercial digital signage solutions that can improve communications in most organizations and corporations. 

### Current features

Display Monkey comprises Management (a.k.a. DMM) and Presentation (a.k.a. DMP) applications. Current features include:

- Frame caching
- Location-specific frames
- Custom frame templates
- Multi-culture, multi-language
- Frames supporting the following content:
  - SQL Reporting Services (SSRS) reports
  - Pictures
  - Local videos
  - YouTube videos
  - Your own HTML
  - Simple memos
  - MSO Outlook calendar with quick booking
  - Text and graphical face clock
  - Yahoo weather
  - Power BI
- WYSIWYG Editor to design presentation layouts
- Management of Displays
- Management of Media (images and videos)
- Online help
- Dashboard
- UI for Application Settings

Check the latest [Change Log](https://github.com/fuel9/DisplayMonkey/blob/master/ChangeLog.md) document for more details.

### Contributing

We encourage contributions. Please feel free to send us [pull requests](https://github.com/fuel9/DisplayMonkey/pulls). We need help with the following:

#### Programming tasks

- Improve DMM navigation flow. Currently server-side request referrer is used as a basis for navigation for CRUD actions. It would be nice to switch to client-side cookies, or similar.
- Fix YT iframe being pegged because of unreleased event handlers installed by YTPlayer causing memory leaks (possible solution [here](http://stackoverflow.com/questions/8948403/youtube-api-target-multiple-existing-iframes)).
- Implement custom ICacheProvider service instead of System.Web.Caching.Cache, which would allow share cache between multiple application pool workers, also double as display activity statistics provider.
- Implement activity graph for DMM home page using D3 or similar library.
- Implement RSS news feed frame type.
- Implement Twitter feed frame type.
- Implement Instagram feed frame type.
- Implement FaceBook feed frame type.
- Implement PowerPoint frame type.
- Implement Panel layer funtion for overlapping panels.
- Implement sorting and paginations in the frame list page grid.
- Implement persistent cookie-based display auto-discovery key instead of the IP address (possible solution [here](http://samy.pl/evercookie/)).
- Transform DMM EF data model from data-first to code-first for simpler maintenance and database deployment.
- Improve frame and panel preview user experience by means of javascript.
- Migrate DMP canvas from Prototype 1.7 to AngularJS.

#### Non-programming tasks

- Translate language resources to other languages.
- Test with browsers not yet known to be supported and provide your valuable feedback.
- Report issues [here](https://github.com/fuel9/DisplayMonkey/issues).
- Spread the word around, give us a star!

### License

Display Monkey is licensed under the terms of MIT License. See full terms [here](https://opensource.org/licenses/MIT).
