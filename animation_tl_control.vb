
Dim resizeAnimationDirector as Director
Dim directorName as String = "BaseScale"
Dim ShmName as String = "shm_resize_dlb"
Dim SceneShmName as String = "scene_shm_resize_dlb"
Dim testParam as String  = "test_anim"
Dim keyFrameStart, KeyFrameEnd as String
Dim state as Integer

sub OnInit()
	resizeAnimationDirector  = Stage.FindDirector(directorName)
	VizCommunication.Map.RegisterChangedCallback(ShmName)
	Scene.Map.RegisterChangedCallback(SceneShmName)
	println(resizeAnimationDirector)
	state = 0
end sub


sub OnInitParameters()
	RegisterParameterInt(testParam, "Trigger Director", 0, 0, 3)
end sub


Sub AnimateFromA2B(aValue as String, bValue as String)
	if resizeAnimationDirector == NULL then
			Exit Sub
	end if
	
	resizeAnimationDirector.GoTo(aValue, bValue)
End Sub

Sub UpdateState(state as Integer)
	if state == 0 then
		AnimateFromA2B("0.5", "0.0")
	elseif state == 1 then
		AnimateFromA2B("0.0", "0.5")
	elseif state == 2 then
		AnimateFromA2B("0.5", "1.0")	
	elseif state == 3 then
		AnimateFromA2B("1.0", "0.5")	
	elseIf state == 4 then
		AnimateFromA2B("0.5", "0.0")	
	elseIf state == 5 then
		AnimateFromA2B("1.0", "1.8")
	elseIf state == 6 then
		AnimateFromA2B("1.75", "2.1")
	elseIf state == 7 then
		AnimateFromA2B("2.1", "1.75")
	end if
End Sub


Sub AnimateState(tempState as Integer)
	if (tempState - state) <= 1 and (tempState - state) >= 0 and tempState <> 3 then
		UpdateState(tempState)
		state = tempState
	elseIf state == 2 and tempState == 1 then
		UpdateState(3)
		state = tempState
	elseIf state == 1  and tempState == 0 then
		UpdateState(4)
		state = tempState
	elseIf state == 2 and tempState == 0 then
		UpdateState(5)
		state = tempState
	elseIf state == 0 and tempState == 3 then
		UpdateState(6)
		state = tempState
	elseIf state == 3 and tempState == 0 then
		UpdateState(7)
		state = tempState
	end if
	
	
End Sub


sub OnParameterChanged(parameterName As String)
	if parameterName  == testParam then
		
		if resizeAnimationDirector == NULL then
			Exit Sub
		end if
		
		Dim tempState = GetParameterInt(parameterName)
		AnimateState(tempState)
	end if
end sub


sub OnSharedMemoryVariableChanged(map As SharedMemory, mapKey As String)
	
	if mapKey == SceneShmName then
		println("I see the changes")
	end if

	if mapKey == ShmName or mapKey == SceneShmName then
		if resizeAnimationDirector == NULL then
			Exit Sub
		end if
		
		Dim temp as String = map[mapKey]
		Dim tempState = CInt(temp)
		
		AnimateState(tempState)
		
'		
'		if state == 1 then
'			resizeAnimationDirector.startAnimation()
'		elseif state == 0 then
'			'resizeAnimationDirector.GoTo("1.3", "1.8")
'			resizeAnimationDirector.continueAnimation()
'		end if
	
	end if
end sub