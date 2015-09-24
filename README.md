#C# client library for ownCloud

A portable class library to interact with ownCloud servers through WebDAV and the ownCloud OCS API.

Project status
==============

The library has all calls present and work is currently focussed on parsing complex OCS API responses. A first usable release is planned for October 1st.

Instructions
============

The project is a .Net portable class library and it should work on .Net and Mono/Xamarin.

ownCloud# uses three libraries webDAVNet, WebClient and RestSharp.

The solution contains a updated and modified version of WebDAVNet that works as a PCL. RestSharp needs to be obtained seperately from the project repository. This is necessary since there are issues building RestSharp as part of a PCL solution using Xamarin Studio. A project reference might be included in the future.

Also required for the WebDAVNet and WebClient projects are the "Microsoft.BCL.Build" and "Microsoft.Net.Http" NuGet packages.

If those dependancies are met the solution should build just fine.

API reference
=============

A up-to-date API reference generated using Doxygen is available in the */doc* folder.

You can view the API reference online [here](https://rawgit.com/bnoffer/owncloud-sharp/master/doc/html/index.html) .

Sample Code
===========

The *ocsharpdemo* project is a basic console application showing how the API works.

Click [here](https://github.com/bnoffer/owncloud-sharp/blob/master/ocsharpdemo/Program.cs) to take a look at the Program.cs online.‚

Credits
=======

ownCloud# relies on the following projects:

* webDAVNet - http://webdavnet.codeplex.com/
* RestSharp - https://github.com/restsharp/RestSharp

This project is inspired by the

* Python client library for ownCloud - https://github.com/owncloud/pyocclient

Authors
=======

Bastian Noffer ( [@bnoffer](https://github.com/bnoffer) )