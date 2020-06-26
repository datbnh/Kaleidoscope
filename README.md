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

_Notes:_
* X-Translation Distance = X-Offset Factor × Template Width _w_
* Y-Translation Distance = Y-Offset Factor × Template Height _h_ 

### Equilateral triangle kaleidoscopes
***Template and flipped template*** (as seeds for the tilable pattern) ***:***

![Template Definition](https://github.com/datbnh/Kaleidoscope/blob/master/Doc/images/Template%20Definition.svg?raw=true)

***Tilable pattern:***
|**Location**                  |  ⑦  |  ⑧  |  ⑨  |  ⑩  |  ⑪  |  ⑫  |  ⑬  |
|:-----------------------------|----:|----:|----:|----:|----:|----:|----:|
|**X-Offset Factor**           |0    |1    |1.5  |1    |1.5  |2.5  |3    |
|**Y-Offset Factor**           |1    |1    |0    |1    |0    |0    |1    |
|**Rotation**                  |-120°|180° |120° |-60° |0°   |60°  |-120°|
|**Template Index**<sup>†</sup>|0    |1    |0    |1    |0    |1    |0    |

<sup>†</sup>: _0: normal template, 1: flipped template_
![Tilable Pattern](https://github.com/datbnh/Kaleidoscope/blob/master/Doc/images/Tilable%20Rectangular%20Pattern.svg?raw=true)
|**Location**                  | ⓪   |  ①  |  ②  |  ③  |  ④  |  ⑤  |  ⑥  |
|:-----------------------------|----:|----:|----:|----:|----:|----:|----:|
|**X-Offset Factor**           |-0.5 |0    |1    |1.5  |2.5  |3    |3    |
|**Y-Offset Factor**           |2    |1    |1    |2    |2    |1    |2    |
|**Rotation**                  |-60° |0°   |60°  |-120°|180° |120° |-60° |
|**Template Index**<sup>†</sup>|1    |0    |1    |0    |1    |0    |1    |

<sup>†</sup>: _0: normal template, 1: flipped template_

### 30°–60°–90° triangle kaleidoscopes
(TBA)

### 45°–45°–90° triangle kaleidoscopes
(TBA)

# Screenshots & Rendered Image
![Screenshot](https://github.com/datbnh/Kaleidoscope/blob/master/Doc/images/U8Idl9qWCc.png?raw=true)
![Screenshot](https://github.com/datbnh/Kaleidoscope/blob/master/Doc/images/cRmuJExQAI.jpg?raw=true)
**Sample rendered image:**
![Rendered](https://github.com/datbnh/Kaleidoscope/blob/master/Doc/images/Kaleidoscope_Romantic%20(10)-1.jpg?raw=true)

# Next steps
The current implementation provides only one mirror system, which is 60°-60°-60° or equilateral triangle.

30°-60°-90° and 45°-90°-45° mirror systems are to be added. 


It is planned to provide the capability of generating animations from specified ranges of input parameters (e.g. offset and angle).
