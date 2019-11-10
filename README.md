# ImgDeDupifyr

## ABOUT
This application is used to discover and determine duplicate images.
This is done in 1 of 3 'request' types. Only 1 request can be active at a given time.
The 3 request types are 
    'Directory',
    'Single',
    and 'Pair'.

Directory - Compares all images in a directory with every other.

Single    - Compares 1 image with all other images in a given directory.

Pair      - Compares 2 different images with each other.

## FORMATS
Requests can be made using the following formats
- Directory
  - '/path/to/some/directory'
- Image with Directory
  - '/path/to/image.[extension], /directory/to/compare against'
- Image with Other Image
  - '/path/to/image1.[extension], /path/to/image2.[extension]'

In the future, I plan to have these called out as parameter value explicitly.

(i.e. 

`-image /image.png -image /image2.png`

or 

`-dir dir/of/images -image /image.png`)

Valid extensions are 
    ".jpg",
    ".jpeg",
    ".jif",
    ".png",
    ".gif",
    ".gifv",
    ".tiff",
    ".bmp",
    and ".webp".

Other extension types will simply be ignore by the application.

## OPTIONS
There are currently 3 options that can be set.

- Directory Level: Tells the program how deep in the directory to search. Does not apply to the Singe request type.
    - Values: all, [top]
- Strictness: How equal the pixel colors must be, to consider them to be equal.
    - Values: Equal, [Fuzzy], Loose
- Bias Factor: The percentage that a comparison must equal, or exceed, for an image to be considered a duplicate.
    - Values: 0 to 100, [80]

(The `[` and `]` around a parameter value denotes it as the default value, if none is supplied explicitly.)

Type
    "options"
    or "o"
to overwrite the current option settings.

## EXAMPLE
Below are some example commands.

`path/to/image/one.png,path/to/image/two.jpg,`

`path/to/image/one.png,directory/of/images`

`directory/of/images`

## NOTES
This text can be view within the application, buy typing 
    "help",
    "h",
    or "?".
