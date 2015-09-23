==============================
C# client library for ownCloud
==============================

This project is inspired by the [Python client library for ownCloud](https://github.com/owncloud/pyocclient) .

Project status
==============

This project is in its very early stages. Get back in a few weeks and we'll see where we are by then.

Instructions
============

The project is a .Net portable class library and it should work on .Net and Mono/Xamarin.

ownCloud# uses three libraries webDAVNet, WebClient and RestSharp.

The solution contains a updated and modified version of WebDAVNet that works as a PCL. RestSharp needs to be obtained seperately from the project repository. This is necessary since there are issues building RestSharp as part of a PCL solution using Xamarin Studio. A project reference might be included in the future.

Also required for the WebDAVNet and WebClient projects are the "Microsoft.BCL.Build" and "Microsoft.Net.Http" NuGet packages.

If those dependancies are met the solution should build just fine.

Credits
=======

ownCloud# relies on the following projects:

* webDAVNet - http://webdavnet.codeplex.com/
* RestSharp - https://github.com/restsharp/RestSharp

Authors
=======

Bastian Noffer (@bnoffer)