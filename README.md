# Canon Dual Fisheye Processing

## Overview
This tool performs fisheye to equirectangular transformations for dual fisheye images created by Canon cameras equipped with a [Canon RF 5.2mm F2.8L DUAL FISHEYE lens](https://www.canon.de/lenses/rf-5-2mm-f2-8l-dual-fisheye-lens/). It serves as an open-source alternative to [Canon's EOS VR Utility](https://www.canon.de/pro/professional-video-solutions/eos-vr-system/vr-utility-adobe-premiere-pro-plugin/).

### Unprocessed Image
![Unprocessed Image](https://github.com/JuliusJacobsohn/CanonVRPreprocessing/blob/main/Sample%20Images/Camera%20Images/IMG_0034.JPG)

### Processed Image
![Processed Image](https://github.com/JuliusJacobsohn/CanonVRPreprocessing/blob/main/Sample%20Images/Camera%20Images%20Processed%20by%20Own%20Tool/IMG_0034.JPG)

## Requirements
- Visual Studio
- .NET 6 SDK

## Getting Started
1. Clone the repository: 
```git clone https://github.com/JuliusJacobsohn/CanonVRPreprocessing.git```
2. Open the solution file CanonVRPreprocessing.sln in Visual Studio.
3. Set CanonVRPreprocessor as the startup project.
4. Build and run the project.

## Usage
The application will prompt for the following:
1. Path of the folder containing unprocessed Canon dual fisheye images.
2. Path of the folder where processed images will be saved.

## How It Works
- The original image is split into two halves, one for each eye.
- Each half is then cropped to a resolution of 4096x4096.
- Hard-coded fisheye circle coordinates (center position and radius) are used to normalize the fisheye circles.
- The fisheye to equirectangular transformation is then applied to these normalized circles.
- The theoretical foundation of this project is strongly influenced by the work of Paul Bourke. For more details on the underlying mathematics, see this [diagram](https://paulbourke.net/dome/dualfish2sphere/diagram.pdf). Additional scientific perspective and critique on the lens can be found [here](https://paulbourke.net/stereographics/Canon_RF_dual_fisheye/).

## Contribution Guidelines
Pull requests are welcome, with particular interest in the following features:
- Adding video support (same transformation logic, additional frame splitting required).
- Adding raw .CR3 support (potentially through integration with [SharpLibraw](https://github.com/laheller/SharpLibraw)).
