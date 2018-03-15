pure C# implementation of [thebennybox's software renderer][1] in < 1000 LOC

![](https:i.imgur.com/8hKZiP0.png)

to celebrate benny's return I decided to go through his tutorials again

you can checkout individual lessons with ```git checkout lesson-12```

lesson branches are kept as close as possible to benny's code, the master
branch is heavily refactored to my own coding style

no particular reason why I used C#, I was just showing a friend how to use
OpenTK to get started with the series and ended up playing with it myself

compile and run with
```sh
msbuild -m -p:Configuration=Release
mono --aot bin/Release/SoftwareRenderer.exe
mono bin/Release/SoftwareRenderer.exe
```

* license
this is free and unencumbered software released into the internal domain.
see the attached UNLICENSE or http:unlicense.org/

[1]: https:www.youtube.com/playlist?list=PLEETnX-uPtBUbVOok816vTl1K9vV1GgH5
