# Firebase.Auth.Unofficial
An unofficial package for Firebase Auth, supporting creating users, logging in users, and validating tokens

## Usage

### Install

Install using Nuget: `Install-Package Firebase.Auth.Unofficial`

### Creating an Account API

To register and get tokens for users, please see the sample [AccountController](https://github.com/KerryRitter/Firebase.Auth.Unofficial/blob/master/TestWebApp/Controllers/AccountController.cs).

### Setting up ASP.NET MVC Core Authorization 

1. Add the following line to the Startup.ConfigureServices() method:

```
services.AddFirebaseAuthorization("my api key");
```

2. Add the following filter to routes to add authorization:

```
[Authorize(Policy = "Firebase")]
```

Please see the TestWebApp for more details.

## Notice

This is using Google Identity Toolkit's APIs directly, the same API's consumed by Firebase's SDKs. These could change! Be careful.

## Credit

Thank you to @rlamasb and his [Firebase.Xamarin](https://github.com/rlamasb/Firebase.Xamarin/) repo, where I "borrowed" a good chunk of code from.