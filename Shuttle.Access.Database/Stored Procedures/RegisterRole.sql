CREATE PROCEDURE [dbo].[RegisterRole]
	@RoleName varchar(130)
AS
	if exists (select null from SystemRole where RoleName = @RoleName)
		return;

	insert into SystemRole
	(
		Id,
		RoleName
	)
	values
	(
		newid(),
		@RoleName
	)