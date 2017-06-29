# SpeccyEngine
A small C# engine to emulate the graphic capabilities of the ZX Spectrum in modern computers

## Why this?

Well, there are two main reasons, first, nostalgia, each 5-10 years I get nostalgic and try to do something related to the ZX Spectrum.

The second reason is because I belive this can be a good tool for beginners.

I can remember when I began programming, I was 7 and I got my Spectrum for Christmas. After playing to the games I had I started reading the ZX Spectrum +2 manual, and for my delight it had one of the best BASIC teaching books, complete with examples. So I started to learn basic the same week I got my speccy, and until today I'm still a programer, I know I wouldn't be one if I didn't got that Spectrum. It's simple capabilities were one of the reasons which kept me learning more and more, in two or three weeks I was writing games (extremely simples, of course, but for a 7 year old kid it was like if I created the eigth wonder) and I loved how the graphics were created, simple and elegantly. Today, one of the hardest things to create for a game are graphics, even simple ones, just blitting two bitmaps on a form is an extremely complex task for an unexperienced programmer, not to say that OpenGL and DirectX are just a nightmare if you aren't an advanced programer.

So, with this engine I pretend to give those inexperienced programers a way to create simple graphics in a simple way, just like the speccy did, nothing ultra wonderful, but functional to begin learning. Of course you can argue there are plenty of game engines, but with those you learn nearly nothing, or the engine is just not programable or you learn how to use some engine clases, not much more, and those for a beginner seem to work like "magic". This engine avoids all of that, you can use the predefined functions of the Basic template, but you also can access the graphic memory and manipulate it as you did on the speccy from assembler.

Also, the basic template has a nice characteristic really good for beginners, it can run code synchronously without interrupting screen refresh. When you are a beginner events, classes and so on can be intimidating (not to speak about multithreading) so you can make a program like on an spectrum, a full ist of code which will execute sequentially but will update screen independently of the code, as the Spectrum hardware did.

Of course, as this is C# and the program is really a C# program you can create anything as powerful as one wants, there's no limitation.

I hope you enjoy the engine, and if you need help leave a comment in the issues section.

(I will document all the functions later ;) )
