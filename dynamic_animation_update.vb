RegisterPluginVersion(0,0,1)

Dim mainAnimationDirector as Director
Dim directorNameParam as String = "director_name"
Dim directorName as String
Dim statesDefParam as String  = "states_graph"
Dim parentContainer as Container  = this
Dim OmoPI as PluginInstance
Dim OmoControlPi as PluginInstance
Dim animationStops as Array[Double]
Dim epsilon as Double = 0.001
Dim prevOmoValue as Integer = -1

Sub OnInit()
	if parentContainer <> NULL Then
		OmoPI = parentContainer.GetFunctionPluginInstance("Omo")
		if OmoPI <> NULL then 
			Println("Omo Found")
		End If
		OmoControlPi = parentContainer.GetFunctionPluginInstance("ControlOmo")
		if OmoControlPi <> NULL then
			Println("Omo Control Found")
		else 
			Println("Omo Control not found")
		end if
	end If
End Sub


Sub OnInitParameters()
	RegisterParameterString(directorNameParam, "Director Name", "", 100, 255, "utf-8")
End Sub


sub OnParameterChanged(parameterName As String)
	if parameterName == directorNameParam then
		directorName = GetParameterString(directorNameParam)
		mainAnimationDirector = Stage.FindDirector(directorName)
		if mainAnimationDirector <> NULL Then
			Println("Director Found")
			Println(directorName)
			animationStops.clear()
			Dim keyFrames as Array[Keyframe]
			mainAnimationDirector.GetKeyFrames(keyFrames)
			Println(keyFrames.Ubound)
			
			For i = 0 To keyFrames.Ubound
				if keyframes[i].Channel == mainAnimationDirector.EventChannel Then
					Println(keyFrames[i].EventValue == ET_STOP)
					Println(Keyframes[i].Time)
					Println(keyframes[i].Channel)
					Println(mainAnimationDirector.EventChannel)
					animationStops.push(keyframes[i].Time)
				end If
			Next
			Println(animationStops.Size)
		Else
			Println("Director not found with name: ")
			Println(directorName)
		end If
	end if
end sub



Sub AnimateFromA2B(dir as Director, aValue as String, bValue as String)
	if dir == NULL then Exit Sub
	dir.GoTo(aValue, bValue)
End Sub

'This will update going forward, only, starting from 0 - max state length
Sub updateStateForwardPass(state as Integer)
	If animationStops.size == 0  then exit sub
	if animationStops.ubound < state then exit sub
	
	if state == 0 then
		AnimateFromA2B(mainAnimationDirector, "0.0", CStr(animationStops[state]-epsilon))
	else
		AnimateFromA2B(mainAnimationDirector, CStr(animationStops[state-1]+epsilon), CStr(animationStops[state]-epsilon))
	End if
End Sub

Sub updateStateReversePass(state as Integer)
	If animationStops.size == 0  then exit sub
	if animationStops.ubound < state then exit sub
	
	if state == 0 then
		AnimateFromA2B(mainAnimationDirector, CStr(animationStops[state+1]-epsilon), CStr(animationStops[state]))
	else
		AnimateFromA2B(mainAnimationDirector, CStr(animationStops[state+1]-epsilon), CStr(animationStops[state]-epsilon))
	End if	
End Sub

sub OnExecPerField()
	if mainAnimationDirector == NULL then Exit Sub
	if OmoPI == NULL then exit Sub
	
	Dim currentOmoValue as Integer =  OmoPI.GetParameterInt("vis_con")
	
	if currentOmoValue <> prevOmoValue and currentOmoValue <= animationStops.ubound Then
		Println(currentOmoValue)
		if currentOmoValue <= prevOmoValue then
			updateStateReversePass(currentOmoValue)
		else
			updateStateForwardPass(currentOmoValue)
		end if
		prevOmoValue = currentOmoValue
	end if
	
end sub

