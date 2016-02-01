# r53export

[![Build Status](https://ci.appveyor.com/api/projects/status/github/Pingfu/r53export?branch=master&svg=true)](https://ci.appveyor.com/project/Pingfu/r53export)

Easily export information from Route53. Periodically snapshot, or backup DNS data from ALL hosted zones

```
C:\>r53export.exe -h

Usage: r53export [-l] [-r name] [-a name] [-u name] [-d name] [-e name] | [-e]

Options:
    -a        Add a new AWS access key.
              Keys are stored in %LocalAppData%\AWSToolkit\RegisteredAccounts.json
              and encrypted by the Windows Data Protection API (DAPI).
    -l        List AWS access key profiles available to the user.
    -e        Export all zone data using the first available AWS access key profile.
    -e name   Export all zone data using a particular AWS access key profile.
    -u name   Update an AWS access key profile.
    -r name   Remove an AWS access key profile.
```