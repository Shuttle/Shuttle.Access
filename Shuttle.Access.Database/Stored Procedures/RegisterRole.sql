CREATE PROCEDURE [dbo].[RegisterRole]
	@Name varchar(130)
AS
	if exists (select null from [Role] where [Name] = @Name)
		return;

	insert into [Role]
	(
		Id,
		[Name]
	)
	values
	(
		newid(),
		@Name
	)