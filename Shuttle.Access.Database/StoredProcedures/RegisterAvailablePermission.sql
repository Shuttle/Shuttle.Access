CREATE PROCEDURE [dbo].[RegisterAvailablePermission]
	@permission varchar(130)
AS
	if exists (select null from AvailablePermission where Permission = @Permission)
		return;

	insert into AvailablePermission
	(
		Permission
	)
	values
	(
		@Permission
	)
