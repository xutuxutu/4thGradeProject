/*! \mainpage

 \image html FABRIK.png
 
 Welcome to Final %IK, the ultimate collection of inverse kinematics solutions for Unity.
 
\section contains Final IK Contains 

 - Full Body %IK system for biped characters
 - Biped %IK - alternate to Unity's built-in Avatar %IK system that provides more flexibility and works in Unity Free with the same API
 - CCD (Cyclic Coordinate Descent) %IK
 - Multi-effector %FABRIK (Forward and Backward Reaching %IK) 
 - Look-At %IK
 - Aim %IK
 - Limb %IK
 - Rotation constraints - Angular, Polygonal (Reach Cone), Spline and Hinge rotation limits that work with CCD, %FABRIK and Aim solvers

\section technicaloverview Technical Overview

 - Does NOT require Unity Pro
 - Does NOT require, but works with Mecanim
 - Written in C#, all scripts are namespaced under %RootMotion and %RootMotion.FinalIK to avoid any naming conflicts with Your existing assets.
 - Tested on Standalone, Web Player, IOS and Android
 - Custom undoable inspectors and scene view handles for each component
 - Warning system to safeguard from null references and invalid setups (will not overflow your console with warnings)
 - Optimized for great performance
 - Modular, easily extendable. Compose your own custom character rigs
 - User manual, HTML documentation, fully documented code
 - Demo scenes and example scripts for all components
 - Tested on a wide range of characters
 
*/


/*! \page page1 User Manual
 * Welcome to Final %IK.
  
  \tableofcontents
  
  \section ikcomponents IK Components
	  Final %IK contains a number of powerful high speed %IK components.
	  
	  \subsection aim Aim
	  	AimIK solver is a modification of the CCD algorithm that rotates a hierarchy of bones to make a child Transform of that hierarchy aim at a target.
	  	It differs from the basic built-in Animator.SetLookAtPosition or the LookAtIK functionality, because it is able to accurately aim transforms that are not aligned to the main axis of the hierarchy.
	  	
	  	AimIK can produce very stabile and natural looking retargeting of character animation, it hence has great potential for use in weapon aiming systems. With AimIK we are able to offset a single forward aiming pose or animation to aim at targets even almost behind the character.
	  	It is only the Quaternion singularity point at 180 degrees offset, where the solver can not know which way to turn the spine. Just like LookAtIK, AimIK provides a clampWeight property to avoid problems with that singularity issue.
	  	
	  	AimIK also works with rotation limits, however it is more prone to get jammed than other constrained solvers, should the chain be heavily constrained.
	  	
	  	Aim provides high accuracy at a very good speed, still it is necessary to keep in mind to maintain the target position at a safe distance from the aiming Transform. 
	  	If distance to the target position is less than distance to the aiming Transform, the solver will try to roll in on itself and might be unable to produce a finite result.

		<b>Getting started:</b>
		- Add the AimIK component to your character
		- Assign the spine bones to "Bones" in the component
		- Assign the Aim Transform (the Transform that you want to aim at the target)
		- Make sure Axis represents the local axis of the Aim Transform that you want to be aimed at the target
		- Set weight to 1, press Play

		<b>Adding AimIK in runtime:</b>
		- Add the AimIK component via script
		- Call AimIK.solver.SetChain()

	  	\image html AimIK.png "The AimIK solver in action"
	  	\image html AimIKComponent.png "The AimIK component"
	  
	  \subsection bipedik Biped IK
	  	%IK system for standard biped characters that is designed to replicate and enhance the behaviour of the Unity's built-in character %IK setup.
	  
	  	BipedIK has many benefits over Unity's Animator %IK. 
	  	<BR>Firstly, Animator %IK does not allow the modifiaction of any of even the most basic solver parameters, such as limb bend direction, 
	  	which makes the system difficult, if not impossible to use or extend in slightly more advanced use cases. Even in the simplest of cases, Animator can produce unnatural poses or bend a limb in unwanted direction and there is nothing that can be done to work around the problem. 
	  	<BR>Secondly, Animator I%K lacks a spine solver.
	  	<BR>Thirdly, Animator's LookAt functionality can often solve to weird poses such as bending the spine backwards when looking over the shoulder.
	  	<BR>BipedIK also incorporates AimIK.
	  	<BR>Last, but not least, BipedIK does NOT require Unity Pro.
	  	
	  	To simplify migration from Unity's built-in Animator %IK, BipedIK supports the same API, so you can just go from animator.SetIKPosition(...) to bipedIK.SetIKPosition(...).
	  	
	  	BipedIK, like any other component in the FinalIK package, goes out of it's way to minimize the work required for set up. 
	  	BipedIK automatically detects the biped bones based on the bone structure of the character and the most common naming conventions, 
	  	so unless you have named your bones in Chinese, you should have BipedIK ready for work as soon as you can drop in the component. If BipedIK fails to recognize the bone references or you just want to change them, you can always manage the references from the inspector.

		<b>Getting started:</b>
		- Add the BipedIK component to the root of your character (the same GameObject that has the Animator/Animation component)
		- Make sure the auto-detected biped references are correct
		- Press play, weigh in the solvers

		<b>Adding BipedIK in runtime:</b>
		- Add the BipedIK component via script
		- Assign BipedIK.references
		- Optionally call BipedIK.SetToDefaults() to set the parameters of the solvers to default BipedIK values. Otherwise default values of each solver are used.

	  	\image html BipedIK.png "Testing BipedIK in the scene view"
	  	\image html BipedIKComponent.png "The BipedIK component"
	  
	  \subsection ccd CCD
	  	CCD (Cyclic Coordinate Descent) is one of the simplest and most popular inverse kinematics methods that has been extensively used in the computer games industry. The main idea behind the solver is to align one joint with the end effector and the target at a time, so that the last bone of the chain iteratively gets closer to the target.
	  	
	  	CCD is very fast and reliable even with rotation limits applied. CCD tends to overemphasise the rotations of the bones closer to the target position. Reducing bone weight down the hierarchy will compensate for this effect.
	  	It is designed to handle serial chains, thus, it is difficult to extend to problems with multiple end effectors (in this case go with FABRIK). It also takes a lot of iterations to fully extend the chain.
	  	
	  	Monitoring and validating the %IK chain each frame would be expensive on the performance, therefore changing the bone hierarchy in runtime has to be followed by calling SetChain (Transform[] hierarchy) on the solver. SetChain returns true if the hierarchy is valid.
	  	
	  	CCD allows for direct editing of it's bones' rotations (not by the scene view handles though), but not positions, meaning you can write a script that rotates the bones in a CCD chain each frame, but you should not try to change the bone positions like you can do with a FABRIK solver.
	  	You can, however, rescale the bones at will, CCD does not care about bone lengths.

		<b>Getting started:</b>
		- Add the CCDIK component to the first GameObject in the chain
		- Assign all the elements in the chain to "Bones" in the component
		- Press Play, set weight to 1

		<b>Adding CCDIK in runtime:</b>
		- Add the CCDIK component via script
		- Call CCDIK.solver.SetChain()
		
	  	\image html CCD.png "CCD with rotation limits applied"
	  	\image html CCDComponent.png "The CCDIK component"
	  	
	  \subsection fabrik FABRIK
	  	Forward and Backward Reaching Inverse Kinematics solver based on the paper: 
	  	<BR><a href="http://andreasaristidou.com/publications/FABRIK.pdf">"FABRIK: A fast, iterative solver for the inverse kinematics problem." </a> 
	  	<BR>Aristidou, A., Lasenby, J. Department of Engineering, University of Cambridge, Cambridge CB2 1PZ, UK.
	  	
	  	FABRIK is a heuristic solver that can be used with any number of bone segments and rotation limits. It is a method based on forward and backward iterative movements by finding a joint's new position along a line to the next joint. 
	  	FABRIK proposes to solve the %IK problem in position space, instead of the orientation space, therefore it demonstrates less continuity under orientation constraints than CCD, although certain modifications have been made to the constraining method described in the original paper to improve solver stability.
	  	It generally takes less iterations to reach the target than CCD, but is slower per iteration especially with rotation limits applied.
	  	
	  	FABRIK is extremely flexible, it even allows for direct manipulation of the bone segments in the scene view and the solver will readapt. Bone lengths can also be changed in runtime if updateBoneLengths parameter is set to true in the solver (false by default for performance reasons).
	  	
	  	Monitoring and validating the %IK chain each frame would be expensive on the performance, therefore changing the bone hierarchy in runtime has to be followed by calling SetChain (Transform[] hierarchy) on the solver. SetChain returns true if the hierarchy is valid.

		<b>Getting started:</b>
		- Add the FABRIK component to the first GameObject in the chain
		- Assign all the elements in the chain to "Bones" in the component
		- Press Play, set weight to 1

		<b>Adding FABRIK in runtime:</b>
		- Add the FABRIK component via script
		- Call FABRIK.solver.SetChain()

	  	\image html FABRIK.png "FABRIK with rotation limits applied"
	  	\image html FABRIKComponent.png "The FABRIK component"
	  	
	  \subsection fabrikroot FABRIK Root
	  	Multi-effector FABRIK system.
	  	<BR>FABRIKRoot is a component that connects FABRIK chains together to form extremely complicated %IK systems with multiple branches, end-effectors and rotation limits.

		<b>Getting started:</b>
		- Create multiple FABRIK chains, position them as you want them to be connected. The chains don't have to be parented to each other
		- Make sure the first bone of a child chain is in the same position as the last bone of it's parent
		- Create a new GameObject, add the FABRIKRoot component
		- Add all the FABRIK chains to "Chains" in the FABRIKRoot component
		- Press Play

		<b>Limitations:</b>
		- Seperate FABRIK chains can not use the same bones, they must be fully independent
		- The last bone of a FABRIK chain must be in the same position as it's child chain's first bone
		
		\image html FABRIKRoot.png "FABRIK Root chain being pulled"
	  	\image html FABRIKRootComponent.png "The FABRIKRoot component"
		
	  \subsection fullbodybiped Full Body Biped IK
	  	Final %IK includes an extremely flexible and powerful high speed lightweight FBIK solver for biped characters.

		FullBodyBipedIK maps any biped character to a low resolution multi-effector IK rig, solves it, and maps the result back to the character. 
		This is done each frame in LateUpdate, after Mecanim/Legacy is done animating, so it is completely independent from the animating system.

		<b>Chains:</b>
		Internally, each limb and the body are instances of the FBIKChain class. The root chain is the body, consisting of a single node, and the limbs are it's children. 
		This setup forms the multi-effector IK tree around the root node.

		<b>Nodes:</b>
		Nodes are members of the Chains. For instance, an Arm chain contains three nodes - upper arm, forearm and the hand. Each node maintains a reference to it's bone (node.transform).
		When the solver is processing or has finished, the solved position of the bone is stored in node.solverPosition.

		<b>Effectors:</b>
		FullBodyBipedIK has three types of effectors - end-effectors (hands and feet), mid-body effectors (shoulders and thighs) and multi-effectors (the body). 
		End-effectors can be rotated while changing the rotation of mid-body and multi-effectors has no effect. Changing end-effector rotation also changes the bending direction of the limb (unless you are using bend goals to override it).
		The body effector is a multi-effector, meaning it also drags along both thigh effectors (to simplify positioning of the body).
		Effectors also have the positionOffset property that can be used to very easily manupulate with the underlaying animation. Effectors will reset their positionOffset to Vector3.zero after each solver update.

		<b>Pulling and Reaching:</b>
		Each chain has the "pull" property. When all chains have pull equal to 1, pull weight is distributed equally between the limbs. That means reaching all effectors is not quaranteed if they are very far from each other. 
		The result can be adjusted or improved by changing the "reach" parameter of the chain, increasing the solver iteration count or updating the solver more than once per frame.
		However, when for instance the left arm chain has pull weight equal to 1 and all others have 0, you can pull the character from it's left hand to Infinity without losing contact.

		<b>Mapping:</b>
		IKSolverFullBodyBiped solves a very low resolution high speed armature. Your character probably has a lot more bones in it's spine though, it might have twist bones in the arms and shoulder or hip bones and so on. Therefore, the solver needs to map the high resolution
		skeleton to the low resolution solver skeleton before solving and vice versa after the solver has finished. There are 3 types of mappers - IKMappingSpine for mapping the pelvis and the spine, IKMappingLimb for the limbs (including the clavicle) and IKMappingBone for the head.
		You can access them through IKSolverFullBody.spineMapping, IKSolverFullBody.limbMappings and IKSolverFullBody.boneMappings

		<b>Limitations:</b>
		- FullBodyBipedIK does not have an effector for the head. That is because the head is just a bone that you can simply rotate however you please after Full Body IK is finished, 
		and there are very few cases, where you would actually need to pull a character from the head. 
		Even then, it could be simulated just as well by pulling the shoulder effectors instead. This is an optimisation that grants us more speed and stability.
		- FullBodyBipedIK does not have effectors for the fingers and toes. Solving fingers with IK would be an overkill in most cases as there are only so few poses for the hands in a game. 
		Using 10 4-segment constrained CCD or FABRIK chains to position the fingers however is probably something you don't want to waste your precious milliseconds on. 
		See the Driving Rig demo to get an idea how to very quickly (and entirely in Unity) pose the fingers to an object.
		- FullBodyBipedIK samples the initial pose of your character (in Start() and each time you re-initiate the solver) to find out which way the limbs should be bent. Hence the limitation - the limbs of the character at that moment should be bent in their natural directions.
		Some characters however are in geometrically perfect T-Pose, meaning their limbs are completely straight. Some characters even have their limbs bent slightly in the inverse direction (some Mixamo rigs for example).
		FullBodyBipedIK will alarm you should this problem occur. All you will have to do, is rotate the forearm or calf bones in the Scene view slightly in the direction they should be bent. 
		Since those rotations will be overwritten in play mode by animation anyway, you should not be afraid of messing up your character.
		- FullBodyBipedIK will not update when animation on the character is not playing (disabled Animator/Animation component).
		- All bones used by the solver should have keyframes in the animation. If the feet of your characters are continuously spinning or the character flies away or gets twisted in a weird way when you weigh in an effector, it is probably because some of the bones are not animated. 
		This does not happen with 3ds Max Biped or any other specialized rig, but if you have a custom generic rig, select all the bones of the character, and set a keyframe for them at frame 0.
		- FullBodyBipedIK does not have elbow/knee effectors. That might change in the future should there be a practical demand for them. Elbow and knee positions can still be modified though as bend goals are supported.
		- Optimize Game Objects should be disabled or at least all the bones needed by the solver (FullBodyBipedIK.references) exposed.
		- Additional bones in the limbs are supported as long as their animation is twisting only. If the additional bones have swing animation, like for example wing bones, FBBIK will not solve the limb correctly.
		- FullBodyBipedIK works with characters that have animatePhysics enabled. There is a bug in Mecanim though, that does not allow for changing animatePhysics in runtime, therefore FullBodyBipedIK will update according to the initial animatePhysics state.
		- FullBodyBipedIK does not rotate the shoulder bone when the character is pulled by the hand. It will maintain the shoulder bone rotation relative to the chest as it is in the animation. 
		In most cases, it is not a problem, but sometimes, especially when reaching for something above the head, having the shoulder bone rotate along would make it more realistic. 
		In this case you should either have an underlaying reach up animation that rotates the shoulder bone or it can also be rotated via script before the IK solver reads the character's pose.
		There is also a workaround script included in the demos, called ShoulderRotator.
		- When you move a limb end-effector and the effector rotation weight is 0, FBBIK will try to maintain the bending direction of the limb as it is animated. When the limb rotates close to 180 degrees from it's animated direction, you will start experiencing rolling of the limb, meaning, the solver has no way to know at this point of singularity, which way to rotate the limb. 
		Therefore if you for example have a walking animation, where the hands are down and you want to use IK to grab something from directly above the head, you will have to take the inconvenience to also animate the effector rotation or use a bend goal, to make sure the arm does not roll backwards when close to 180 degrees of angular offset. 
		This is not a bug, it is a logical inevitability if we want to maintain the animated bending direction by default.
		- FullBodyBipedIK considers all elbow and knee joints as hinge joints, meaning the forearm can not twist relative to the upper arm. With the most common rigs, like the 3ds Max Biped, it is not a problem, as the rig prevents this kind of animation from the start.
		There are rigs however that allow for that kind of twisting. It will not break the FBBIK solver, instead, FBBIK will force the elbow or knee to restrain to a hinge joint even if there are no effectors enabled. It will not change the end position of the limb, it might slightly change the bending direction of the limb and the twist rotation of the bones in the limb.

		<b>Getting started:</b>
		- Add the FullBodyBipedIK component to the root of your character (the same GameObject that has the Animator/Animation component)
		- Make sure the auto-detected biped references are correct
		- Make sure the Root Node was correctly detected. It should be one of the bones in the lower spine.
		- Take a look at the character in the scene view, make sure you see the FullBodyBipedIK armature on top the character.
		- Press Play, weigh in the solvers

		<b>Adding FullBodyBipedIK in runtime:</b>
		- Add the FullBodyBipedIK component via script
		- call FullBodyBipedIK.SetReferences();

		<b>Optimizing FullBodyBipedIK:</b>
		- You can use renderer.isVisible to weigh out the solver when the character is not visible.
		- Most of the time you don't need so many solver iterations and spine mapping iterations. Note that with just 1 iteration character shoulders and thighs might get dislocated when pulled from both hands/feet
		- Keep the "Reach" values at 0 if you don't need them. By default they are 0.05f to improve accuracy.
		- Keep the Spine Twist Weight at 0 if you don't see the need for it.
		- Also setting the "Spine Stiffness", "Pull Body Vertical" and/or "Pull Body Horizontal" to 0 will help the performance.

		\image html FullBodyBipedIK.png "Retargeting a single punching animation with FullBodyBipedIK"
	  	\image html FullBodyBipedIKComponent.png "The FullBodyBipedIK component"

	  \subsection limb Limb
	  	LimbIK extends TrigonometricIK to specialize on the 3-segmented hand and leg character limb types.
	  	LimbIK comes with multiple Bend Modifiers:
	  		- Animation: tries to maintain bend direction as it is in the animation
	  		- Target: rotates the bend direction with the target IKRotation
	  		- Parent: rotates the bend direction along with the parent Transform (pelvis or clavicle)
	  		- Arm: keeps the arm bent in a biometrically natural and relaxed way (also most expensive of the above).
	  	
	  	If none of the automatic bend modifiers fit your needs, you can always create bend goal objects. It is as easy as:
	  	
\code
	using RootMotion.FinalIK;

	public LimbIK limbIK;
	
	void LateUpdate () {
		limbIK.solver.SetBendGoalPosition(transform.position);
	}
\endcode
		This will make the limb bend towards the direction from the first bone to the position of the goal.
		
		The IKSolverLimb.maintainRotationWeight property allows to maintain the world space rotation of the last bone fixed as it was before solving the limb. 
		<BR>This is most useful when we need to reposition a foot, but maintain it's rotation as it was animated to ensure proper alignment with the ground surface.

		<b>Getting started:</b>
		- Add the LimbIK component to the root of your character (the character should be facing it's forward direction)
		- Assign the limb bones to bone1, bone2 and bone3 in the LimbIK component
		- Press Play

		<b>Adding LimbIK in runtime:</b>
		- Add the LimbIK component via script
		- Call LimbIK.solver.SetChain()

	  	\image html LimbIK.png "LimbIK with bend goal"
	  	\image html LimbIKComponent.png "The LimbIK component"

	  \subsection lookat LookAt
		LookAt IK can be used on any character or other hierarchy of bones to rotate a set of bones to face a target. 

<b>Getting started:</b>
		- Add the LookAtIK component to the root GameObject. That GameObject's forward axis will be the forward direction.
		- Assing Spine, head and eye bones to the component.
		- Press Play

		<b>Adding LookAtIK in runtime:</b>
		- Add the LookAtIK component via script
		- Call LookAtIK.solver.SetChain()

		\image html LookAtIK.png "LookAtIK in action"
	  	\image html LookAtIKComponent.png "The LookAtIK component"
		
	  \subsection trigonometric Trigonometric
	  Trigonometric IK is the most basic IK solver that is based on the Law of Cosines and solves a 3-segmented bone hierarchy.

<b>Getting started:</b>
		- Add the TrigonometricIK component to the first bone.
		- Assign bone1, bone2 and bone3 in the TrigonometricIK component
		- Press Play

		<b>Adding TrigonometricIK in runtime:</b>
		- Add the TrigonometricIK component via script
		- Call TrigonometricIK.solver.SetChain()

		\image html TrigonometricIK.png "Solving a 3-segment chain with TrigonometricIK"
	  	\image html TrigonometricIKComponent.png "The TrigonometricIK component"
	  
  \section rotationlimits Rotation Limits
  
  All rotation limits and other Final %IK components are Quaternion and Axis-Angle based to ensure consistency, continuity and to minimize singularity issues. Final %IK does not contain a single Euler operation. 
  <BR>All rotation limits are based on local rotations and use the initial local rotation as reference just like Physics joints. This makes them axis-independent and intuitive to set up.
  <BR>All rotation limits have undoable Scene view editors. 
  <BR>All rotation limits work with %IK solvers that support rotation limits.
  
  \image html RotationLimits.png "Rotation Limits"
  
  \subsection angle Angle
  	Simple angular swing and twist limit.
  
  	\image html RotationLimitAngle.png "The anglular rotation limit"
  	\image html RotationLimitAngleComponent.png "The RotationLimitAngle component"
  
  \subsection hinge Hinge
  
  The hinge rotation limit limits the rotation to a single degree of freedom around an axis. This rotation limit is additive which means the hinge limits can exceed 360 degrees either way.
  
  \image html RotationLimitHinge.png "Adjusting hinge limits in the scene view"
  \image html RotationLimitHingeComponent.png "The RotationLimitHinge component"
  
  \subsection polygonal Polygonal
  	Using a spherical polygon to limit the range of rotation on universal and ball-and-socket joints. A reach cone is specified as a spherical polygon 
  	on the surface of a a reach sphere that defines all positions the longitudinal segment axis beyond the joint can take. 
  	<BR>The twist limit parameter specifies the maximum twist around the main axis.
  	
  	This class is based on the paper:
  	<BR><a href="http://users.soe.ucsc.edu/~avg/Papers/jtl.pdf">"Fast and Easy Reach-Cone Joint Limits" </a> 
  	<BR>Jane Wilhelms and Allen Van Gelder. Computer Science Dept., University of California, Santa Cruz, CA 95064. August 2, 2001
  	
  	The polygonal rotation limit is provided with handy scene view tools for quick editing, cloning and modifying of the reach cone points.
  	
  	\image html RotationLimitPolygonal.png "Defining reach cone points on the polygonal rotation limit"
  	\image html RotationLimitPolygonalComponent.png "RotationLimitPolygonal component"
  
   \subsection spline Spline
   
   Using a spline to limit the range of rotation on universal and ball-and-socket joints. 
   <BR>Reachable area is defined by an AnimationCurve orthogonally mapped onto a sphere, which provides a very smooth and fast result.
   <BR>The twist limit parameter specifies the maximum twist around the main axis.
   	
   The spline rotation limit is provided with handy scene view tools for quick editing, cloning and modifying of the spline handles.	
   
   \image html RotationLimitSpline.png "Adjusting spline handles on on the spline rotation limit"
   \image html RotationLimitSplineComponent.png "The RotationLimitSpline component"
   
  \section extending Extending Final IK
  	The %IK solvers and rotation limits of FinalIK were built from the ground up with extendability in mind. 
  	<BR>Some of the components of FinalIK, such as BipedIK, are essentially little more than just collections of %IK solvers.

  	\subsection customcomponents Writing Custom IK Components
  		Before you can exploit the full power of FinalIK, it is important to know a few things about it's architecture.
  		
  		The difference between %IK components and %IK solvers:
  		<BR> By architecture, %IK solver is a class that actually contains the inverse kinematics functionality, while the function of an %IK component is only to harbor, initiate and update it's solver and provide helpful scene view handles as well as custom inspectors.  
  		<BR> Therefore, %IK solvers are fully independent of their components and can even be used without them through direct reference:
  		
\code
using RootMotion.FinalIK;

public IKSolverCCD spine = new IKSolverCCD();
public IKSolverLimb limb = new IKSolverLimb();

void Start() {
	// The root transform reference is used in the initiation of IK solvers for multiple reasons depending on the solver.
	// heuristic solvers IKSolverCCD, IKSolverFABRIK and IKSolverAim only need it as context for logging warnings, 
	// character solvers IKSolverLimb, IKSolverLookAt, BipedIK and IKSolverFullBodyBiped use it to define their orientation relative to the character,
	// IKSolverFABRIKRoot uses it as the root of all of it's FABRIK chains.
	spine.Initiate(transform);
	limb.Initiate(transform);
}

void LateUpdate() {
	// Updating the IK solvers in a specific order.
	// In the case of multiple IK solvers handling a bone hierarchy, it is usually wise to solve the parents first.
	spine.Update();
	limb.Update();
}
\endcode
		You now have essentially a custom %IK component.
		<BR>This can be helpful if you needed to keep all the functionality of your %IK system in a single component, like BipedIK, so you would not have to manage many different %IK components in your scene.
  	
  	\subsection customrotationlimits Writing Custom Rotation Limits
  	All rotation limits in Final %IK extend from the abstract RotationLimit class. To compose your own, you would as well need to extend from this base class and override the abstract method 
  	\code 
  	protected abstract Quaternion LimitRotation(Quaternion rotation); 
  	\endcode
  	
  	In this method you will have to apply the constraint to and return the input Quaternion.
  	<BR>It is important to note that the input Quaternion is already converted to the default local rotation space of the gameobject, meaning if you return Quaternion.identity, the gameobject will always remain fixed to it's initial local rotation.
  	
  	The following code could be a template for a custom rotation limit:
  	
\code
using RootMotion.FinalIK;

// Declaring the class and extending from RotationLimit.cs
public class RotationLimitCustom: RotationLimit {
	
	// Limits the rotation in the local space of this instance's Transform.
	protected override Quaternion LimitRotation(Quaternion rotation) {		
		return MyLimitFunction(rotation);
	}

}

\endcode
	The new rotation limit gets recognized and applied automatically by all constrainable %IK solvers.
  	
  	\subsection combining Combining IK Components
  		When creating more complex %IK systems, you will probably need full control over the updating order of your solvers. To do that, you can just disable their components and manage their solvers from an external script.
  		<BR>All %IK components extend from the abstract IK class and all %IK solvers extend from the abstract IKSolver class. This enables you to easily handle or replace the solvers even without needing to know the specific type of the solver.
  		<BR>Controlling the updating order of multiple %IK components:
\code
using RootMotion.FinalIK;

// Array of IK components that you can assign from the inspector. 
// IK is abstract, so it does not matter which specific IK component types are used.
public IK[] components;
  	
void Start() {
	// Disable all the IK components so they won't update their solvers. Use Disable() instead of enabled = false, the latter does not guarantee solver initiation.
	foreach (IK component in components) component.Disable();
}

void LateUpdate() {
	// Updating the IK solvers in a specific order. 
	foreach (IK component in components) component.GetIKSolver().Update();
}
\endcode

*/

/*! \page page2 FAQ
 * Coming soon...
*/