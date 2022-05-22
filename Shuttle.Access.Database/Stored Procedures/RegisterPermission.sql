CREATE PROCEDURE [dbo].[RegisterPermission]
	@Name varchar(130),
	@Status int
AS
	if exists (select null from Permission where [Name] = @Name)
		return;

	insert into [Permission]
	(
		Id,
		[Name],
		[Status]
	)
	values
	(
		newid(),
		@Name,
		@Status
	)