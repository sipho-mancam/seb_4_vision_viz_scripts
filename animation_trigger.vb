Dim shm_address = "dlb_move_up_shm"
Dim shm_address_cut_back = "dlb_move_up_shm_cut_back"
Dim directorName as String
Sub OnInit()
	VizCommunication.Map.RegisterChangedCallback(shm_address)
end Sub


sub OnInitParameters()
	RegisterParameterInt("trigger_animation", "Animation In/Out", 0, 0 , 1)
	RegisterParameterString("director_name", "Director Name", "director", 100, 100, "")
end sub


sub OnParameterChanged(parameterName As String)
	if parameterName == "trigger_animation" then
		Dim val as Integer = GetParameterInt(parameterName)
		if val == 1 then
			TriggerAnimation(False)
		else
			TriggerAnimation(True)
		end if
	ElseIf parameterName == "director_name" then
		directorName = GetParameterString(parameterName)
		println(directorName)
	end if
end sub




Sub TriggerAnimation(cutBack as Boolean)
	Dim dir as Director  = Stage.FindDirector(directorName)
	
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
		TriggerAnimation(False)
	end if
	
	if mapKey == shm_address_cut_back then
		TriggerAnimation(True)
	end if

end sub

