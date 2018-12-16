C# client library for ownCloud
==============================

A portable class library to interact with ownCloud servers through WebDAV and the ownCloud OCS API.

Project status
==============

[![Coverity Scan Build Status](https://scan.coverity.com/projects/6488/badge.svg)](https://scan.coverity.com/projects/bnoffer-owncloud-sharp)

Current code base has been tested to work on:

* ownCloud 8.2 - 10
* NextCloud 12

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

You can view the API reference online [here](https://combinatronics.com/bnoffer/owncloud-sharp/master/doc/html/index.html) .

Sample Code
===========

The *ocsharpdemo* project is a basic console application showing how the API works.

Click [here](https://github.com/bnoffer/owncloud-sharp/blob/master/ocsharpdemo/Program.cs) to take a look at the Program.cs online.‚

Unit Tests
==========

The *owncloud-sharp-test* project contains all unit tests for ownCloud# and relies on the NUnit Framework. In order to perform the unit tests the NUnit NuGet Package is required.

**NOTE:** The *owncloud-sharp-test* project contains a class *TestSettings* that contains the configuration for the ownCloud v.8.2+ server installation to be used for testing. Ideally this should be a development server installation. You can find instructions on how to setup a development installation of ownCloud [here](https://doc.owncloud.org/server/8.2/developer_manual/general/devenv.html).

ownCloud Version 8.2 or higher is required for all tests to complete successfully. This is because the OCS Share API only supports Federated Cloud Shares starting with version 8.2.

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