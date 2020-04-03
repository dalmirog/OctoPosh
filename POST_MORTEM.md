

# Octoposh Post Mortem

Octoposh was a side project I worked on from 2015 to 2017. Back then I was working as a Technical Support Engineer @ Octopus Deploy and I was very passionate about Powershell and integrating apps using APIs. So I started building this PS Module that interacted with Octopus Deploy to automate some of the REST API actions that Octopus' customers were asking about the most.

The last time I pushed a commit here was in December 2017,  and at that moment this project reflected my best game as a Software Developer (which wasn't much since my background was mostly as a DevOps). Looking at it 3+ years later, it kind of makes me cringe because of how low the code quality is compared to what I could do in the present. But at the same time, it makes me proud to see I was able to carry out all the layers of this project from scratch by myself.

These are some of the parts of the project I was most proud of:

- **The code of the module**
    - The first versions of the module (`0.1` - `0.5`) were built 100% in Powershell. But after writing the first 10 cmdlets, I quickly noticed I needed to use a stronger language to achieve what I wanted (E.g. a better test suite). So I learned C# and in `0.6` I ported the entire module to this language.

    - **If I had to do this from scratch again** I'd definitely do it with C# (with the extra years of experience I have now!) and .NET Core so the module could also be used in Unix systems.

- **CI/CD**
    - For the automated builds I was running `Cake` scripts on `TeamCity`. The reason why I went with `Cake` instead of having that logic on separate `TeamCity` steps, is because I wanted to be able to test/debug the whole build process locally, instead of having to wait for a TC build to run->fail to see what went wrong. Plus, having the whole process in a file allowed me to store it on Github. See [Cake script for the powershell Module](https://github.com/dalmirog/OctoPosh/blob/master/PowershellModule_Build.cake)

    - For automated deployments I used `Octopus Deploy`. At the end of each build, `TeamCity` ended up pushing a `zip` package containing the PS Module to the `Octopus` built-in repository, and then Octopus published that to the [Powershell Gallery](https://www.powershellgallery.com/packages/Octoposh/0.6.11). Octopus was also in charge of publishing the Octoposh.Net website (no longer online) to `Azure`. See the (very basic) [ASP.NET Web App project](https://github.com/dalmirog/OctoPosh/tree/master/Octoposh.Web).

- **Automated Tests**
    - It was my first time using a fully fledged testing framework such as `NUnit` (I had only used `Pester` at the moment), so I had to learn a lot of new concepts related to Unit Testing. But most importantly, I had to learn how to write actually testable code! I was pretty happy with the % of test coverage I ended up having. See [Octopost.Tests](https://github.com/dalmirog/OctoPosh/tree/master/Octoposh.Tests)

    - Each Octopus object (i.e a `Deployment`) returned by the API had a lot of dependencies on other objects, so there were moments where creating mock objects just wasn't enough, and you needed real data. So I ended up writing a script that spun an Octopus Instance and filled it with test data created by another console app. See [TestDataGenerator](https://github.com/dalmirog/OctoPosh/tree/master/Octoposh.TestDataGenerator)

    - **If I had to do this from scratch again** I'd have a Docker container running Octopus with the test data already set up that I could easly re-build as needed to test against it. 
    
- **Documentation**
    - I always enjoyed Powershell's built-in documentation, so I put a lot of effort into including examples for each cmdlet so users wouldn't have to google for them. See [cmdlet built-in help example](https://github.com/dalmirog/OctoPosh/blob/master/Octoposh/Cmdlets/GetOctopusEnvironment.cs#L12-L29)

    - But I also knew some people would rather see the documentation from a browser, so I set up a [Read The Docs page](https://octoposh.readthedocs.io/en/latest/) for the Module. These docs were automatically generated using a script that pulled the built-in documentation mentioned above, dropped it in a series of JSON files and then pushed them into [the docs folder]((https://github.com/dalmirog/OctoPosh/tree/master/docs)) that `Read The Docs` was feeding from. You can see that script [over here](https://github.com/dalmirog/OctoPosh/blob/master/Scripts/Generate-CmdletDocs.ps1).

- **Community side**
    - In one of my previous jobs I used to heavily use [Chocolatey](https://chocolatey.org/), and I liked they way they used `Gitter` to engage with its community and provide help (this was before Slack was a thing). So I started a [Gitter channel for the project](https://gitter.im/Dalmirog/OctoPosh?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge), where I was able to help a lot of users (and get many bug reports!)

    - Since I was a Tech Support Engineer for `Octopus Deploy` and I had a direct line with Octopus' customers through the public forums, I started promoting this Powershell Module wherever i saw fit (this was all previously agreed with the Octopus team, of course). This way I got a lot a new users adopting the module into their daily tasks, which motivated me to continue working on it. In the following link you can see some of the threads in the Octopus public help forum where users asked about `Octoposh`: https://help.octopus.com/search?q=octoposh



The mains reasons why I stopped working on this project were:
- The `Octopus.Client` library that this module depended on and the `Octopus Server` product itself both moved so quickly, that it was too hard to keep up with them with this module. So at a certain point, I couldn't find a strong reason to keep working on this Powershell project, when users could simply import the `Octopus.Client` .NET library into their PS scripts and achieve something similar by themselves.

- This project started as a way for me to level up my Development game so I could start leaning a bit more towards that side of my DevOps role. In 2018 I started getting development tasks in the company I was working for, so I barely had any free left to work on this project.