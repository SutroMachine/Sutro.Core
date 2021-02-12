# gsGCode

GCode parsing / manipulation / generation library. 

C#, MIT License. Copyright 2017 ryan schmidt / gradientspace

Dependencies: [geometry3Sharp](https://github.com/gradientspace/geometry3Sharp) (Boost license)

questions? get in touch on twitter: [@rms80](http://www.twitter.com/rms80) or [@gradientspace](http://www.twitter.com/gradientspace), 
or email [rms@gradientspace.com](mailto:rms@gradientspace.com?subject=gsGCode).

## What Is This?

gsGCode is a library for working with GCode. Not just generating GCode, but also reading and manipulating existing GCode files.

What? Edit a GCode file? that's crazy, right? CAD people used to think the same thing about STL mesh files too.
STL was something you wrote out to communicate with a CNC machine. You might "repair" it, but you didn't edit it.
But then a guy wrote [OpenSCAD](http://www.openscad.org/), and I wrote [Meshmixer](http://www.meshmixer.com), and now STL
files are something that thousands of people open and edit every day.

The purpose of this library is to make the same thing possible for GCode. GCode is just 3D lines, curves, and
control commands. There is no reason you can't open a GCode file, change one of the paths, and write it back out. 
You can easily do that with this library.

## Can It Write GCode Too?

Yes! I'm developing gsGCode to support the gradientspace Slicer, which is not released yet. But the low-level GCode generation 
infrastructure is all here. You can either use **GCodeBuilder** to construct a **GCodeFile** line-by-line, or an Assembler 
(currently only **MakerbotAssembler**) which provides a higher-level turtle-graphics-like API. 

## How?

See [the wiki](https://github.com/gradientspace/gsGCode/wiki) for what minimal documentation exists right now.
Some sample code will come in time. 

# gsSlicer

**In-Progress** Slicer for 3D printing, and other toolpath-type things, perhaps.

C#, MIT License (*but see notes below!*). Copyright 2017 ryan schmidt / gradientspace

questions? get in touch on twitter: [@rms80](http://www.twitter.com/rms80) or [@gradientspace](http://www.twitter.com/gradientspace), 
or email [rms@gradientspace.com](mailto:rms@gradientspace.com?subject=gsSlicer).

Gradientspace no longer offers consulting services. If you are interested in using gsSlicer and would like to hire someone to provide advice, help with the code, or  develop things for you, get in touch with Sutro Machine (http://sutromachine.com), they have extensive experience with gsSlicer.


## What is this?

gsSlicer is an in-development open-source library for things like slicing 3D triangle meshes into planar polygons, filling those polygons with contour & raster fill paths, figuring out how much material to extrude along the paths, and then outputting GCode. The included **SliceViewer** project is also a GCode viewer/utility.

The goal with this project is to create a well-structured slicing engine that is designed from the ground up to be extensible. Although the initial focus will be on FDM/FFF-style printers, many of the parts of the system will be applicable to other processes like SLA, etc. Hopefully. Fingers crossed. At least, we'll definitely solve the meshes-to-slices problem for you.


## Current Status

**Under Active Development**. Generated GCode has been used for non-trivial prints, however the output has not been extensively tested. 

Shells, solid and sparse infill, roof and floors, have been implemented. Support volumes calculated but not yet filled. **ThreeAxisPrintGenerator** is top-level driver for FDM printing.

Experimental support for SLS contours & hatching is available in **GenericSLSPrintGenerator**. 

**Supported Printers**: At this time, only Makerbot Replicator 2 has been significantly tested. Also has been tested with Monoprice Select Mini (and hence should work with any generic RepRap) and Printrbot Metal Plus.


## Usage

This project is a source code library, not usable directly. GUI and command-line front ends are under development but very basic at this point, in the [gsSlicerApps](https://github.com/gradientspace/gsSlicerApps) project.

For an example of how to convert a mesh into GCode, see **GenerateGCodeForMeshes()** in *gsSlicerApps/sliceViewGTK/SliceViewerMain.cs*


## Dependencies

The slicing & path planning library **gsSlicer** depends on:

* [geometry3Sharp](https://github.com/gradientspace/geometry3Sharp) Boost license, git submodule
* [Clipper](http://www.angusj.com/delphi/clipper.php) by Angus Johnson, Boost license, embedded in /gsSlicer/thirdparty/clipper_library

No GPL/LGPL involved. All the code you would need to make an .exe that slices a mesh is available for unrestricted commercial use.


