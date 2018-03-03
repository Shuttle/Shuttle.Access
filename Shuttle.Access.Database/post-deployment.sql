/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
exec RegisterAvailablePermission 'access://roles/manage'
exec RegisterAvailablePermission 'access://roles/view'
exec RegisterAvailablePermission 'access://users/manage'
exec RegisterAvailablePermission 'access://users/view'
exec RegisterAvailablePermission 'access://dashboard/view'

exec RegisterRole 'Anonymous'
exec RegisterRolePermission 'Anonymous', 'access://dashboard/view'