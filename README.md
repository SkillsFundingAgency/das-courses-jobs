## ⛔Never push sensitive information such as client id's, secrets or keys into repositories including in the README file⛔

# das-courses-jobs

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_apis/build/status/das-courses-jobs?branchName=main)](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_build/latest?definitionId=3941&branchName=main)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-courses-jobs&metric=alert_status)](https://sonarcloud.io/dashboard?id=SkillsFundingAgency_das-courses-jobs)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

```
A collection of Azure functions which perform background processing in the Courses API.

Store individual json documents in a GitHub repository for the IFATE published standards to maintain a searchable history of changes to IFATE data over time. Using 
a GitHub repository allows history and change comparison to be made between dates.
```

## 🚀 Installation

### Pre-Requisites

```
* A clone of this repository
* A code editor that supports Azure functions and .NetCore 8.0
* A GitHub repository which is a fork of T.B.D*
```
### Config

```
This utility uses the standard Apprenticeship Service configuration. All configuration can be found in the [das-employer-config repository](https://github.com/SkillsFundingAgency/das-employer-config).

* GitHub repository name which is a fork of.
* GitHub username having contributor access to the repository.
* GitHub access token which is a fine-grained personal access token created by the above user with access to the above repository.
```

AppSettings.Development.json file
```json
{
    "Logging": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
      }
    },
    "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true;",
    "ConfigNames": "SFA.DAS.Courses.Jobs",
    "EnvironmentName": "LOCAL",
    "Version": "1.0",
    "APPINSIGHTS_INSTRUMENTATIONKEY": ""
  }  
```

Azure Table Storage config

Row Key: SFA.DAS.Courses.Jobs_1.0

Partition Key: LOCAL

Data:

```json
{
  "GitHubRepositoryName": "",
  "GitHubUserName": "",
  "GitHubAccessToken": ""
}
```

## Technologies

_List the key technologies in-use in the project. This will give an indication as to the skill set required to understand and contribute to the project_

_For Example_
```
* .NetCore 8.1
* Azure Functions V4
* NLog
* Azure Table Storage
* NUnit
* Moq
* FluentAssertions
```

## 🐛 Known Issues

```
* None
```