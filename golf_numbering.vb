Dim shm_address = "dlb_move_up_shm"
Dim shm_address_cut_back = "dlb_move_up_shm_cut_back"
Dim listItemsArr as Array[Container]
Dim listParent as Container

Sub OnInit()
	VizCommunication.Map.RegisterChangedCallback(shm_address)
	listParent = scene.FindContainer("LIST")
end Sub


sub OnInitParameters()
	RegisterParameterInt("trigger_shift_up", "Shift Up", 0, 0 , 1)
	
	println(Scene)
end sub


sub OnParameterChanged(parameterName As String)
	println(parameterName)
	if parameterName == "trigger_shift_up" then
		Dim val as Integer = GetParameterInt(parameterName)
		if val == 1 then
			ActivateContainers(True)
			TriggerAnimation(False)
		else
			ActivateContainers(False)
			TriggerAnimation(True)
		end if
	end if
end sub


Sub ActivateContainers(activate as Boolean)
	Dim parentContainer as Container = Scene.FindContainer("LIST")
	
	if parentContainer <> NULL then
		Dim childrenCount as Integer = parentContainer.ChildContainerCount 
		if activate <> False then
			parentContainer.ShowAllChildContainers()
		else
			for i = 20	 to childrenCount 
				Dim child as Container = parentContainer.GetChildContainerByIndex(i)
				child.Active = False
			Next
		end if
	end if	
end Sub


Sub TriggerAnimation(cutBack as Boolean)
	Dim dir as Director  = Stage.FindDirector("YScale")
	
	if dir <> NULL then
		if cutBack == False then
			dir.StartAnimation()
		
		else
			dir.ContinueAnimation()	
		end if
		
	else
		println("Director No Found")
	end if
		
end Sub

sub OnSharedMemoryVariableChanged(map As SharedMemory, mapKey As String)
	if mapKey == shm_address then 
		ActivateContainers(True)
		TriggerAnimation(False)
	end if
	
	if mapKey == shm_address_cut_back then
		TriggerAnimation(True)
		ActivateContainers(False)
	end if
end sub


Sub activateAll(parentContainer as Container)
	Dim numberOfChildren = parentContainer.ChildContainerCount
	Dim i as Integer
	Dim tempContainer as Container
	
	for i=0 To numberOfChildren-1
		tempContainer = parentContainer.GetChildContainerByIndex(i)

		if tempContainer <> NULL then	
			Dim tempPosition as Container = tempContainer.FindSubContainer("Position")
			tempPosition.Active=True
		end if
	Next		

End Sub


sub updateNumbering(parentContainer as Container)
	activateAll(parentContainer)

	Dim numberOfChildren = parentContainer.ChildContainerCount
	Dim i as Integer
	Dim j as Integer
	Dim tempContainer as Container
	Dim pivotElement as Container
	
	
	
	for j = 0 To numberOfChildren-2
		pivotElement  = parentContainer.GetChildContainerByIndex(j)
		
		for i=j+1 To numberOfChildren-1
			tempContainer = parentContainer.GetChildContainerByIndex(i)
			
			if tempContainer <> NULL then
				Dim pivotPosition as Container = pivotElement.FindSubContainer("Position")
				Dim tempPosition as Container = tempContainer.FindSubContainer("Position")
				
				Dim pvtNumber as Container  = pivotPosition.FirstChildContainer
				Dim tmpNumber as Container  = tempPosition.FirstChildContainer
					
				if CInt(pvtNumber.Geometry.Text) == CInt(tmpNumber.Geometry.Text)  then
					tempPosition.Active=False
				end if
				
			end if
		Next
	Next

end sub


sub OnExecPerField()
	listParent = scene.FindContainer("LIST")
	if listParent <> NULL then
		updateNumbering(listParent)
	end if

end sub
