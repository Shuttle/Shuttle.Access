CREATE PROCEDURE [dbo].[RegisterRolePermission]
	@RoleName varchar(130),
	@PermissionName varchar(130)
AS
	declare @RoleId uniqueidentifier
	declare @PermissionId uniqueidentifier

	select @RoleId = Id from Role where [Name] = @RoleName

	if (@RoleId is null)
		return 0;

	select @PermissionId = Id from Permission where [Name] = @PermissionName

	if (@PermissionId is null)
		return 0;

	if exists 
	(
		select 
			null 
		from 
			RolePermission
		where 
			RoleId = @RoleId 
		and 
			@PermissionId = @PermissionId
	)
		return;

	insert into RolePermission
	(
		RoleId,
		PermissionId
	)
	values
	(
		@RoleId,
		@PermissionId
	)