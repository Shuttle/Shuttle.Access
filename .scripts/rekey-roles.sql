UPDATE
    access.IdKey
SET
    UniqueKey = '[role]:name=' + r.Name + ';tenant-id=' + LOWER(CAST(r.TenantId AS VARCHAR(36)))
FROM
    access.Role r
INNER JOIN
    access.IdKey ik ON ik.Id = r.Id