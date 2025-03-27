import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import os
import glob
from mpl_toolkits.mplot3d.art3d import Line3DCollection
import matplotlib.cm as cm
import matplotlib.colors as mcolors
import matplotlib as mpl

'''
file_path_single = "11-02-2025_11-43-22.csv"
data_single = load_csv_to_numpy(file_path_single)

fig = plt.figure()
ax = plt.axes(projection='3d')
ax.set_xlim([0, 500])
ax.set_ylim([0, 500])
ax.set_zlim([0, 200])
#ax.plot3D(data_single[0], data_single[2], data_single[1])
script_dir = os.path.dirname(os.path.abspath(__file__))
csv_files = glob.glob(os.path.join(script_dir, "*.csv"))
for file_path in csv_files:
    data = load_csv_to_numpy(file_path)
    ax.plot3D(data[0], data[2], data[1], label=os.path.basename(file_path))
'''

'''
TestingDiver (1.)   212.271439,119.490868,219.693787
Diver1              281.881012,36.8699989,294.869995
Diver2              87.8600006,23.5,94.5
Diver3              50.2999992,40.2999992,442.5
Diver4              443.540009,39.0499992,90.5899963
Diver5              427.019989,60.6500015,438.25
base                207.893082,131.025192,187.043793

11-02-2025 10-24-56 Testing 1 5 base
11-02-2025 11-43-22 Testing 1 5 base 2
11-02-2025 13-52-54 Testing 1 base 2
16-12-2024 14-07-40 Testing 1 base
19-12-2024 11-54-36 Testing 1 3 base 2
'''

def load_csv_to_numpy(file_path):
    df = pd.read_csv(file_path)
    data_array = df.to_numpy()
    return data_array.transpose()

diver_positions = np.array([
    [212.271439,119.490868,219.693787],
    [281.881012,36.8699989,294.869995],
    [87.8600006,23.5,94.5],
    [50.2999992,40.2999992,442.5],
    [443.540009,39.0499992,90.5899963],
    [427.019989,60.6500015,438.25]
    ])
diver_positions = np.transpose(diver_positions)

base_position = np.array([207.893082,131.025192,187.043793])
base_position = np.transpose(base_position)

positions_for_plot = np.array([
    [212.271439,119.490868,219.693787],
    [281.881012,36.8699989,294.869995],
    [87.8600006,23.5,94.5],
    [50.2999992,40.2999992,442.5],
    [443.540009,39.0499992,90.5899963],
    [427.019989,60.6500015,438.25],
    [207.893082,131.025192,187.043793]
    ])
positions_for_plot = np.transpose(positions_for_plot)

script_dir = os.path.dirname(os.path.abspath(__file__))
csv_files = glob.glob(os.path.join(script_dir, "*.csv"))
plt.rcParams["figure.dpi"] = 200

for file_path in csv_files:
    data = load_csv_to_numpy(file_path)
    fig = plt.figure(figsize=(15, 5))

    ax = fig.add_subplot(131, projection='3d')
    ax.set_xlim([0, 500])
    ax.set_ylim([0, 500])
    ax.set_zlim([0, 200])

    #ax.plot3D(data[0], data[2], data[1], color='blue', linewidth=2, label="Trajectory")
    # Compute the segments for the trajectory
    points = np.array([data[0], data[2], data[1]]).T  # Convert to (N, 3)
    segments = np.array([points[:-1], points[1:]]).transpose(1, 0, 2)  # Line segments
    
    # Normalize distance for colormap scaling
    norm = mcolors.Normalize(vmin=np.min(data[4]), vmax=np.max(data[4]))  
    cmap = mpl.colormaps["viridis"]  # Choose a colormap
    colors = [cmap(norm(d)) for d in data[4][:-1]]  # Map distance to colors

    # Create a colored 3D line collection
    line = Line3DCollection(segments, colors=colors, linewidth=2, label="Trajectory")
    ax.add_collection3d(line)  # Add trajectory to plot

    ax.plot3D(data[0], data[2], 0, color='grey', linewidth=1, alpha=0.25)
    ax.scatter3D(diver_positions[0], diver_positions[2], diver_positions[1], color='red')
    ax.scatter3D(base_position[0], base_position[2], base_position[1], color='yellow')
    ax.scatter3D(positions_for_plot[0], positions_for_plot[2], 0, color='grey', alpha=0.25)
    ax.set_title(os.path.basename(file_path))

    ax2 = fig.add_subplot(132, projection='3d')
    ax2.set_xlim([0, 500])
    ax2.set_ylim([0, 500])
    ax2.set_zlim([0, 200])

    ax2.plot3D(data[0], data[2], data[1], color='blue', linewidth=2, label="Trajectory")
    #ax2.add_collection3d(line)

    ax2.plot3D(data[0], data[2], 0, color='grey', linewidth=1, alpha=0.25)
    ax2.scatter3D(diver_positions[0], diver_positions[2], diver_positions[1], color='red')
    ax2.scatter3D(base_position[0], base_position[2], base_position[1], color='yellow')
    ax2.scatter3D(positions_for_plot[0], positions_for_plot[2], 0, color='grey', alpha=0.25)
    ax2.view_init(elev=40, azim=0)
    ax2.set_title("azimut 0")

    ax3 = fig.add_subplot(133, projection='3d')
    ax3.set_xlim([0, 500])
    ax3.set_ylim([0, 500])
    ax3.set_zlim([0, 200])

    ax3.plot3D(data[0], data[2], data[1], color='blue', linewidth=2, label="Trajectory")

    ax3.plot3D(data[0], data[2], 0, color='grey', linewidth=1, alpha=0.25)
    ax3.scatter3D(diver_positions[0], diver_positions[2], diver_positions[1], color='red')
    ax3.scatter3D(base_position[0], base_position[2], base_position[1], color='yellow')
    ax3.scatter3D(positions_for_plot[0], positions_for_plot[2], 0, color='grey', alpha=0.25)
    ax3.view_init(elev=40, azim=90)
    ax3.set_title("azimut 90")

plt.show()