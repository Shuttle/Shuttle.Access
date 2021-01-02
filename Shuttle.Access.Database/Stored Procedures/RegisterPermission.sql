CREATE PROCEDURE [dbo].[RegisterPermission]
	@Permission varchar(130)
AS
	if exists (select null from Permission where Permission = @Permission)
		return;

	insert into [Permission]
	(
		Permission
	)
	values
	(
		@Permission
	)