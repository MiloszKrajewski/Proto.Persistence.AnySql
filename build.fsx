#r ".fake/FakeLib.dll"
#load "build.tools.fsx"

open Fake

let build () = Proj.build "src"
let restore () = Proj.restore "src"
let pack project = Proj.pack 

Target "Clean" (fun _ -> !! "**/bin/" ++ "**/obj/" |> DeleteDirs)

Target "Restore" (fun _ -> restore ())

Target "Build" (fun _ -> build ())

Target "Rebuild" ignore

Target "Release" (fun _ -> Proj.releaseNupkg ())

"Restore" ==> "Build"
"Build" ==> "Rebuild"
"Clean" ?=> "Restore"
"Clean" ==> "Rebuild"
"Rebuild" ==> "Release"

RunTargetOrDefault "Build"