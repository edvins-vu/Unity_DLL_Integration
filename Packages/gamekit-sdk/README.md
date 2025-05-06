# Package template

This package is not intended to be pulled in a prototype or any other project other than used as a starting point.

### Layout

https://docs.unity3d.com/Manual/cus-layout.html

### Assembly naming

https://docs.unity3d.com/Manual/cus-asmdef.html

### Versioning

We are using `semantic` versioning. More about that in [semver.org](https://semver.org/) or Unity official documentation https://docs.unity3d.com/Manual/upm-semver.html  
Versions should not be manually updated. There is automated pipeline that will take care of that.

#### Publishing new version

Initial release is manually published as `v1.0.0` and git tag is created.  
There is naming convention that must be followed in order to automatically trigger pipeline to increase the version.  
When pushing directly to master (which is not recommended), use word in front of the commit message (or after task-id)  

- `PATCH` to increment patch version `0.0.X`
- `MINOR` to increment minor version (used when new feature or functions are added) `0.X.0`
- `MAJOR` to increment major version (when package became not backwards compatible) `X.0.0`

The secondary method and the method that is prefered is to use `merge request`

Create branches according to feature you would like to make

- Patch version `patch/*`
- Minor version `minor/*`
- Major version `major/*`

Replace * with given task id followed by the readable naming you prefer to use.  
Example: `minor/PCKG-1-ExampleText`

Upon successul merge to `master` branch pipeline will be triggered and new version will be released automatically.  
In order to validate that new version has been released, check if the pipeline finished successfully. If you do not have access to see pipeline, then check out if new git tag with corresponding version has been created.

Visit [Estoty Riga package registry](http://pkgs.estoty.games:4873/) to see if new version has also been added to registry. Registry access is `authenticated only` and if you do not have user then contact responsible person (your team lead / tech lead) to gain access.

### Changelog

Each package should contain information about changelog using `CHANGELOG.md` file in the root directory.  
We are using `keep changelog` standard that can be found in https://keepachangelog.com/en/1.1.0/

When new version will be released and merge, its important that changelog is created with information about what is changed in order to keep track of new features or bugfixes.  
Changelog also can be updated with versions that are deprecated marking them as such to avoid using them.

### New package

If you want to create new package then simply 

- Create empty Unity project (or use existing one for packages)
- Using `terminal` (recommended) to change directory to Unity project and its `Packages` sub-folder. Example: `cd C:/Code/Estoty/PackagesProject/Packages`
- Clone this repository `git clone git@bitbucket.org:estotyofiss/package-template.git` in the `Packages` directory
- Delete existing `.git` directory to avoid accident pushes to `template repository`
- Rename `AssemblyDefinitions`, `folder names`, `namespaces` according to Layout and Namings described above
- Create new git repository and follow instructions to push initial code
- :warning: WRITE TESTS

When the package is considered as a release version `v1.0.0` contact your `team lead`, `teach lead` or responsible `devops` person to release the initial version of the package.  
Beaware that after the initial version has been released it should be considered as `used in production`.  
This means that team is using the package and all subsequent changes should be both developed and versioned according to release plan and semantic versioning described above.  