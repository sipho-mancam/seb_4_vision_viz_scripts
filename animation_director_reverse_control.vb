' The way to use this script, is if you want to have 
' An In_Out Director as the root Director
' An ANUP_xxxxx director as the update Director
' The ANUP_XXXX director can start in reverse, or in forward direction
' Based on the start command sent,
' The start command takes this form:
' start:<number(4,5)>
' if number == 4 The ANUP_XXX director will start in reverse
' if number == 5 The ANUP_XXX director will start in forward motion
' To send single commands like continue, and take out 
' Use the SharedMemory Variable "animation_director_control"
' To Send the start pair command which is start:<number>
' User the SharedMemory Variable "animation_director_control_pair" 

Structure AnimationState
	dir as Director
	direction as Boolean ' False is forward, True is reverse
	current as Boolean ' Used to check if this is the current selected Director
	animationStarted as Boolean ' Used to check if the animation started
	Name as String
End Structure

Structure SHMCommands
	startAnimation as Integer 
	updateAnimation as Integer 
	takeOutAnimation as Integer 
	updateReverseAnimation as Integer 
	updateForwardAnimation as Integer
End Structure

Dim animationStates as Array[AnimationState]
Dim currentAnimationState as AnimationState
Dim animCommands as SHMCommands

Sub OnInit()
	'AnimationUpdateDirectors() must be called only once, or it resets the currentAnimationState to
	' first element by default everytime it's called.
	Dim updateDirList as Array[AnimationState] = AnimationUpdateDirectors()
	animationStates = updateDirList
	VizCommunication.Map.RegisterChangedCallback("animation_director_control")
	VizCommunication.Map.RegisterChangedCallback("animation_director_control_pair")
	
	animCommands.startAnimation = 1
	animCommands.updateAnimation = 2
	animCommands.takeOutAnimation = 3
	animCommands.updateReverseAnimation = 4
	animCommands.updateForwardAnimation = 5
	
	VizCommunication.Map["animation_director_control_pair"] = CStr(-1)
	VizCommunication.Map["animation_director_control"] = CStr(-1)
	
End Sub 

Sub OnInitParameters()
	RegisterPushButton("continue_anim", "Continue Animation", 1)
	RegisterPushButton("start_anim", "Start Animation", 2)
	RegisterPushButton("take_out_animation", "Take Out Animation", 3)
	
	RegisterParameterBool("reverse_current_animation_state", "Reverse Current Update", False)
	RegisterParameterLabel("current_animation_state", "Current Animation State", 0,0)
End Sub

Sub OnParameterChanged(parameterName as String)
	If parameterName.Match("reverse_current_animation_state") Then
		currentAnimationState.direction = GetParameterBool("reverse_current_animation_state")
		currentAnimationState.animationStarted = False
	End If
End Sub


Sub OnSharedMemoryVariableChanged(map as SharedMemory, mapKey as String)
	
	Println("Received Map Key: "&mapKey)	
	
	If mapKey =="animation_director_control" Then
		Dim value as String = map[mapKey]
		Dim command as Integer = CInt(value)
		Println("Received Command: "&CStr(command))
		If command == animCommands.startAnimation Then
			Stage.StartAnimation()
		ElseIf command == animCommands.updateAnimation Then
			Update()
		ElseIf command == animCommands.takeOutAnimation Then
			Stage.RootDirector.ContinueAnimation()
		ElseIf command == animCommands.updateReverseAnimation Then
			currentAnimationState.direction = True
			currentAnimationState.animationStarted = False
			Println("Current Animation Direction"&CStr(currentAnimationState.direction))
		ElseIf command == animCommands.updateForwardAnimation Then
			currentAnimationState.direction = False
			currentAnimationState.animationStarted = False
		End If
		
	ElseIf mapKey == "animation_director_control_pair" Then
		Dim value as String  = map[mapKey]
		Dim commandStrings as Array[String]
		value.Split(":", commandStrings)
		Dim directionCommand as String = commandStrings[1]
		Dim direction as Integer  = CInt(directionCommand)
		If direction <> 4 And direction <> 5 Then Exit Sub
		
		If direction == 4 Then
			' Reverse the animation
			currentAnimationState.direction = True
			currentAnimationState.animationStarted = False
		Else
			currentAnimationState.direction = False
			currentAnimationState.animationStarted = False
		End If
		Stage.StartAnimation()
	End If
	
	map[mapKey] = CStr(-1)
	
End Sub

Sub OnExecAction(buttonId as Integer)
	If buttonId == 1 Then
		Update()
	ElseIf buttonId == 2 Then
		Stage.StartAnimation()
	ElseIf buttonId == 3 Then
		Stage.RootDirector.ContinueAnimation()
	End If 
End Sub



Function CollectDirectors() As Array[Director]
	Dim currentDir as Director
	Dim listOfDirectors as Array[Director]
	currentDir = Stage.RootDirector
	do
		If currentDir == Null Then
			CollectDirectors=listOfDirectors
			Exit Function 
		End If
		listOfDirectors.Push(currentDir)
		currentDir = currentDir.NextDirector
	Loop While currentDir <> Null
	
	CollectDirectors = listOfDirectors

End Function

Function ExtractUpdateDirectors(listOfDirs as Array[Director]) as Array[Director]
	Dim updateDirList as Array[Director]
	Dim nameSplit as Array[String]
	
	For Each dir In listOfDirs
		dir.Name.Split("_",nameSplit)
		nameSplit[0].MakeLower()
		If nameSplit[0].Match("anup") Then
			updateDirList.Push(dir)
		End If
		
	Next
	
	ExtractUpdateDirectors = updateDirList
End Function


Function AnimationUpdateDirectors() As Array[AnimationState]
	
	Dim dirList as Array[Director] = CollectDirectors()
	Dim updateDirList as Array[Director] = ExtractUpdateDirectors(dirList)
	Dim animStates as Array[AnimationState]	
	Dim currentState as AnimationState
	
	For Each dir In updateDirList
		currentState.dir = dir
		currentState.direction = False
		currentState.current = False
		currentState.animationStarted=False
		currentState.Name = dir.Name
		animStates.Push(currentState)
	Next
	
	
	If animStates.ubound <> -1 Then
		currentState = animStates[0]
		currentState.direction = True
		currentState.current = True
		currentAnimationState = currentState
	End If
	
	
	AnimationUpdateDirectors = animStates
End Function


Sub Update()
	If currentAnimationState.direction Then
	 	' Reverse Animation 
		currentAnimationState.dir.ContinueAnimationReverse()
	Else
		' Forward Animation
		currentAnimationState.dir.ContinueAnimation()
	End If
End Sub

Sub CheckReverseUpdate()
	Dim rev as Boolean = GetParameterBool("reverse_current_animation_state")
	If currentAnimationState.dir <> Null  and currentAnimationState.direction <> rev Then
		currentAnimationState.direction = rev
	End IF
End Sub


Sub OnExecPerField()

	If Stage.RootDirector.IsAnimationRunning() And (currentAnimationState.dir <> Null) And Not currentAnimationState.animationStarted Then
		'Reverse the animation		
		If currentAnimationState.direction Then		
			currentAnimationState.dir.StartAnimationReverse()
			currentAnimationState.animationStarted = True
		Else
			currentAnimationState.dir.StartAnimation()
			currentAnimationState.animationStarted = True
		End If
	End If
	
End Sub



