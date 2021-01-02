CREATE PROCEDURE [dbo].[RegisterRole]
	@RoleName varchar(130)
AS
	if exists (select null from [Role] where RoleName = @RoleName)
		return;

	insert into [Role]
	(
		Id,
		RoleName
	)
	values
	(
		newid(),
		@RoleName
	)