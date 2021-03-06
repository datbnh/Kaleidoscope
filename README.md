# Kaleidoscope

> A kaleidoscope (/kəˈlaɪdəskoʊp/) is an optical instrument with two or more reflecting surfaces tilted to each other in an angle, so that one or more (parts of) objects on one end of the mirrors are seen as a regular symmetrical pattern when viewed from the other end, due to repeated reflection. 
--https://en.wikipedia.org/wiki/Kaleidoscope

This software extracts a part of a specified image as a template and simulates the pattern as being viewed through a kaleidoscope.

# Developer Notes
## Tilable rectangular pattern generation
To simplify the final image generation process and in order to generalise different kaleidoscope types into an interface, a _tilable rectangular pattern_ is generated by each type of kaleidoscope itself after the template is extracted from the input image. After having this rectangular pattern, the image renderer just needs to tile it certain times to fill the target image size.

In this approach, the implementation of each kaleidoscope type takes care of the generation of the tilable pattern which involves different set of rotations and translations specific to certain kaleidoscope type.

This is not an optimal solution in terms of performance, as the _tilable rectangular pattern_ is not neccessary an _atomic tilable pattern_. Yet, an atomic tilable pattern requires involving rotations and translations when generating the final image, which is, again, specific to certain type of kaleidoscope.

The transformation (e.g. rotations and translations) set of each kaleidoscope used in the implementation of the current version is documented in the "magic table" below.

_Notes:_ these conventions are used:
* X-Translation Distance = X-Offset Factor × Template Width _w_
* Y-Translation Distance = Y-Offset Factor × Template Height _h_ 
* Template Index: 0 = original template, 1 = mirrored template
* Template size is always its width _w_, height _h_ is derrived parameter depending on kaleidoscope type

### Equilateral triangle kaleidoscopes
***Template and flipped template*** (as seeds for the tilable pattern) ***:***
![Template Definition](https://github.com/datbnh/Kaleidoscope/blob/master/Doc/images/TemplateDefinition_60-60-60.svg?raw=true)

***Tilable pattern:***
![Tilable Pattern](https://github.com/datbnh/Kaleidoscope/blob/master/Doc/images/TilablePattern_60-60-60.svg?raw=true)

|**Location**        |  ⑦  |  ⑧  |  ⑨  |  ⑩  |  ⑪  |  ⑫  |  ⑬  |
|:-------------------|----:|----:|----:|----:|----:|----:|----:|
|**X-Offset Factor** |0    |1    |1.5  |1    |1.5  |2.5  |3    |
|**Y-Offset Factor** |1    |1    |0    |1    |0    |0    |1    |
|**Rotation**        |-120°|180° |120° |-60° |0°   |60°  |-120°|
|**Template Index**  |0    |1    |0    |1    |0    |1    |0    |

|**Location**        | ⓪   |  ①  |  ②  |  ③  |  ④  |  ⑤  |  ⑥  |
|:-------------------|----:|----:|----:|----:|----:|----:|----:|
|**X-Offset Factor** |-0.5 |0    |1    |1.5  |2.5  |3    |3    |
|**Y-Offset Factor** |2    |1    |1    |2    |2    |1    |2    |
|**Rotation**        |-60° |0°   |60°  |-120°|180° |120° |-60° |
|**Template Index**  |1    |0    |1    |0    |1    |0    |1    |

### 30°–60°–90° triangle kaleidoscopes
***Template and flipped template*** (as seeds for the tilable pattern) ***:***
![Template Definition](https://github.com/datbnh/Kaleidoscope/blob/master/Doc/images/TemplateDefinition_30-60-90.svg?raw=true)

***Tilable pattern:***
![Tilable Pattern](https://github.com/datbnh/Kaleidoscope/blob/master/Doc/images/TilablePattern_30-60-90.svg?raw=true)

|**Location**        | ⓪   |  ①  |  ②  |  ③  |  ④  |  ⑤  |  ⑥  |  ⑦  |  ⑧  |  ⑨  |  ⑩  |  ⑪  |
|:-------------------|----:|----:|----:|----:|----:|----:|----:|----:|----:|----:|----:|----:|
|**X-Offset Factor** |0    |1    |1.5  |1.5  |2    |3    |4    |4.5  |5    |4    |4.5  |5    |
|**Y-Offset Factor** |0    |0    |0.5  |0.5  |1    |1    |1    |0.5  |0    |1    |0.5  |0    |
|**Rotation**        |0    |60   |60   |-120 |-120 |180  |180  |120  |120  |-60  |-60  |0    |
|**Template Index**  |0    |1    |0    |0    |1    |0    |1    |0    |1    |1    |0    |1    |

|**Location**        | ⓿   |  ❶  |  ❷  |  ❸  |  ❹  |  ❺  |  ❻  |  ❼  |  ❽  |  ❾  |  ❿  |  ⓫  |
|:-------------------|----:|----:|----:|----:|----:|----:|----:|----:|----:|----:|----:|----:|
|**X-Offset Factor** |1    |1.5  |2    |1    |1.5  |2    |3    |4    |4.5  |4.5  |5    |6    |
|**Y-Offset Factor** |2    |1.5  |1    |2    |1.5  |1    |1    |1    |1.5  |1.5  |2    |2    |
|**Rotation**        |180  |120  |120  |-60  |-60  |0    |0    |60   |60   |-120 |-120 |180  |
|**Template Index**  |1    |0    |1    |1    |0    |1    |0    |1    |0    |0    |1    |0    |

### 45°–45°–90° triangle kaleidoscopes
***Template and flipped template*** (as seeds for the tilable pattern) ***:***
![Template Definition](https://github.com/datbnh/Kaleidoscope/blob/master/Doc/images/TemplateDefinition_45-90-45.svg?raw=true)

***Tilable pattern:***
![Tilable Pattern](https://github.com/datbnh/Kaleidoscope/blob/master/Doc/images/TilablePattern_45-90-45.svg?raw=true)

|**Location**        | ⓪   |  ①  |  ②  |  ③  |  ④  |  ⑤  |  ⑥  |  ⑦  |
|:-------------------|----:|----:|----:|----:|----:|----:|----:|----:|
|**X-Offset Factor** |0    |1    |1    |0    |0    |1    |1    |0    |
|**Y-Offset Factor** |0    |0    |2    |2    |0    |0    |2    |2    |
|**Rotation**        |0    |90   |180  |270  |0    |90   |180  |270  |
|**Template Index**  |0    |1    |0    |1    |1    |0    |1    |0    |

|**Location**        |  ⑧  |  ⑨  |  ⑩  |  ⑪  |  ⑫  |  ⑬  |  ⑭  |  ⑮  |
|:-------------------|----:|----:|----:|----:|----:|----:|----:|----:|
|**X-Offset Factor** |0    |1    |1    |0    |0    |1    |1    |0    |
|**Y-Offset Factor** |2    |2    |4    |4    |2    |2    |4    |4    |
|**Rotation**        |0    |90   |180  |270  |0    |90   |180  |270  |
|**Template Index**  |1    |0    |1    |0    |0    |1    |0    |1    |

# Screenshots & Rendered Image
![Screenshot](https://github.com/datbnh/Kaleidoscope/blob/master/Doc/images/U8Idl9qWCc.png?raw=true)
![Screenshot](https://github.com/datbnh/Kaleidoscope/blob/master/Doc/images/cRmuJExQAI.jpg?raw=true)
**Sample rendered image:**
![Rendered](https://github.com/datbnh/Kaleidoscope/blob/master/Doc/images/Kaleidoscope_Romantic%20(10)-1.jpg?raw=true)

# Next steps
It is planned to provide the capability of generating animations from specified ranges of input parameters (e.g. offset and angle).
