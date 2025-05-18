# Shuttle.Access

An Identity and Access Management (IAM) platform.

# Documentation

Please visit the [Shuttle.Access documentation](https://www.pendel.co.za/shuttle-access/home.html) for more information

- move "allow identity/password" setting to back-end
- add description to identity (for OID scenarios)
- logging of tokens as option
- configuration.json:

```json
{
    "Roles": [
        {
            "Name": "Admin",
            "Description": "Administrator role",
            "Permissions": [
                "system://name/permission-a"
            ]
        },
        {
            "Name": "User",
            "Description": "User role"
        }
    ],
    "Permissions": [
        "system://name/permission-a",
        "system://name/permission-b",
    ]
}
```