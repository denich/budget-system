USE [MyCompany_Database];
GO

IF OBJECT_ID('[dbo].[usp_YearComplexBudgetProjectSelect]') IS NOT NULL
BEGIN 
    DROP PROC [dbo].[usp_YearComplexBudgetProjectSelect] 
END 
GO
CREATE PROC [dbo].[usp_YearComplexBudgetProjectSelect] 
    @ID INT
AS 
	SET NOCOUNT ON 
	SET XACT_ABORT ON  

	BEGIN TRAN
	
	SELECT * 
	FROM   [dbo].[YearComplexBudget] ybud
	INNER JOIN [dbo].[ComplexlBudget] cbud ON ybud.ComplexBudgetID = cbud.ID
	INNER JOIN [dbo].[BudgetProject] prj ON prj.ComplexBudgetID = cbud.ID
	WHERE  (ybud.ComplexBudgetID = @ID OR @ID IS NULL) 

	COMMIT
GO
IF OBJECT_ID('[dbo].[usp_YearComplexBudgetProjectInsert]') IS NOT NULL
BEGIN 
    DROP PROC [dbo].[usp_YearComplexBudgetProjectInsert] 
END 
GO
CREATE PROC [dbo].[usp_YearComplexBudgetProjectInsert] 
    @AdministrativeUnitID int,
    @Year int,
    @Revision int,
    @RevisionDate datetime,
    @UpdatePersonId int,
    @IsAccepted bit
AS 
	SET NOCOUNT ON 
	SET XACT_ABORT ON  
	
	BEGIN TRAN
	
	DECLARE @ID [int];
	INSERT INTO [dbo].[ComplexlBudget] ([AdministrativeUnitID])
	SELECT @AdministrativeUnitID
	
	SELECT @ID = SCOPE_IDENTITY();
		
	INSERT INTO [dbo].[YearComplexBudget] ([ComplexBudgetID], [Year])
	SELECT @ID, @Year
	
	INSERT INTO [dbo].[BudgetProject] ([ComplexBudgetID], [Revision], [RevisionDate], [UpdatePersonId], [IsAccepted])
	SELECT @ID, @Revision, @RevisionDate, @UpdatePersonId, @IsAccepted
	
	-- Begin Return Select <- do not remove
	SELECT * 
	FROM   [dbo].[YearComplexBudget] ybud
	INNER JOIN [dbo].[ComplexlBudget] cbud ON ybud.ComplexBudgetID = cbud.ID
	INNER JOIN [dbo].[BudgetProject] prj ON prj.ComplexBudgetID = cbud.ID
	WHERE  (ybud.ComplexBudgetID = @ID OR @ID IS NULL) 
	-- End Return Select <- do not remove
               
	COMMIT
GO
IF OBJECT_ID('[dbo].[usp_YearComplexBudgetProjectUpdate]') IS NOT NULL
BEGIN 
    DROP PROC [dbo].[usp_YearComplexBudgetProjectUpdate] 
END 
GO
CREATE PROC [dbo].[usp_YearComplexBudgetProjectUpdate] 
    @ID int,
    @AdministrativeUnitID int,
    @Year int,
    @Revision int,
    @RevisionDate datetime,
    @UpdatePersonId int,
    @IsAccepted bit
AS 
	SET NOCOUNT ON 
	SET XACT_ABORT ON  
	
	BEGIN TRAN

	UPDATE [dbo].[ComplexlBudget]
	SET    [AdministrativeUnitID] = @AdministrativeUnitID
	WHERE  [ID] = @ID
	
	UPDATE [dbo].[YearComplexBudget]
	SET    [Year] = @Year
	WHERE  [ComplexBudgetID] = @ID
	
	UPDATE [dbo].[BudgetProject]
	SET    [Revision] = @Revision, [RevisionDate] = @RevisionDate, [UpdatePersonId] = @UpdatePersonId, [IsAccepted] = @IsAccepted
	WHERE  [ComplexBudgetID] = @ID
	
	-- Begin Return Select <- do not remove
	SELECT * 
	FROM   [dbo].[YearComplexBudget] ybud
	INNER JOIN [dbo].[ComplexlBudget] cbud ON ybud.ComplexBudgetID = cbud.ID
	INNER JOIN [dbo].[BudgetProject] prj ON prj.ComplexBudgetID = cbud.ID
	WHERE  (ybud.ComplexBudgetID = @ID OR @ID IS NULL) 
	-- End Return Select <- do not remove

	COMMIT
GO
IF OBJECT_ID('[dbo].[usp_YearComplexBudgetProjectDelete]') IS NOT NULL
BEGIN 
    DROP PROC [dbo].[usp_YearComplexBudgetProjectDelete] 
END 
GO
CREATE PROC [dbo].[usp_YearComplexBudgetProjectDelete] 
    @ID int
AS 
	SET NOCOUNT ON 
	SET XACT_ABORT ON  
	
	BEGIN TRAN
	
	DELETE
	FROM   [dbo].[ComplexlBudget]
	WHERE  [ID] = @ID
	
	/*Can delete this when add foreign key*/
	DELETE
	FROM   [dbo].[YearComplexBudget]
	WHERE  [ComplexBudgetID] = @ID
	
	DELETE
	FROM   [dbo].[BudgetProject]
	WHERE  [ComplexBudgetID] = @ID

	COMMIT
GO

----------------------------------------------------------------------------------------
----------------------------------------------------------------------------------------

