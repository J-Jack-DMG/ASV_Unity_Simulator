# AVS_Unity_Simulator

The content of this repository is the product of my thesis's work.
This project is intended to provide a unified framework to easily train an ASV agent in a simulated environment.

## Features

![image](./Images/Env_presentation.png)

This is what the environment looks like.
In the hierarchy on the left, the principal components are:
- The world 
- The target
- The water manager
- The boat

The first two contain the structure of the environment.
Instead, the water manager and the boat are the objects that contain the C# script that defines the wave configuration and the interaction between the vessel and the water, respectively.

### Water manager scripts

![image1](./Images/water_manager_settings_presentation.png)

The water manager contains two scripts.
The first one includes the variables that define the water plane's size and resolution.
Instead, the second script manages the parameters that modify the wave form.

### Boat scripts
![image2](./Images/boat_script_presentation.png)
![image3](./Images/boat_manager_settings_presentation.png)

## Contributions

Before any push check the dimension of each file using the bash command explaind [here](https://netshopisp.medium.com/how-to-find-large-files-and-directories-in-linux-server-b176698d276f#:~:text=The%20%2Dtype%20f%20option%20specifies,details%20for%20each%20file%20found.)

The command to run is reported below:

```
  find . -type f -size +100M -exec ls -lh {} \; | awk '{ print $5 ": " $NF }' | sort -n -r
```
