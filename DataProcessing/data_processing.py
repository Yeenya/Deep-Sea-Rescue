import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import os
import glob
from mpl_toolkits.mplot3d.art3d import Line3DCollection
import matplotlib.cm as cm
import matplotlib.colors as mcolors
import matplotlib as mpl
import scipy.spatial.transform as tf

'''
TestingDiver (1.)   212.271439,119.490868,219.693787
Diver1              281.881012,36.8699989,294.869995
Diver2              87.8600006,23.5,94.5
Diver3              50.2999992,40.2999992,442.5
Diver4              443.540009,39.0499992,90.5899963
Diver5              427.019989,60.6500015,438.25
base                207.893082,131.025192,187.043793

11-02-2025 10-24-56 Testing 1 5 base (0, 1, 5, 6)
11-02-2025 11-43-22 Testing 1 5 base 2 (0, 1, 5, 6, 2)
11-02-2025 13-52-54 Testing 1 base 2 (0, 1, 6, 2)
16-12-2024 14-07-40 Testing 1 base (0, 1, 6)
19-12-2024 11-54-36 Testing 1 3 base 2 (0, 1, 3, 6, 2)
'''

ordering = {}
ordering["11-02-2025_10-24-56.csv"] = [0, 1, 5, 6]
ordering["11-02-2025_11-43-22.csv"] = [0, 1, 5, 6, 2]
ordering["11-02-2025_13-52-54.csv"] = [0, 1, 6, 2]
ordering["16-12-2024_14-07-40.csv"] = [0, 1, 6]
ordering["19-12-2024_11-54-36.csv"] = [0, 1, 3, 6, 2]
ordering["23-03-2025_21-33-31.csv"] = [0, 1, 5, 3, 2, 4, 6] #jeste baze mezi 4 a 6, mozna i mezi 2 a 4
ordering["25-03-2025_19-17-19.csv"] = [0, 1, 5, 2, 4, 3, 6] #nejake baze mezi

def load_csv_to_numpy(file_path):
    df = pd.read_csv(file_path)
    data_array = df.to_numpy()
    return data_array.transpose()

def convert_quat_to_vector(quaternion):
    # Convert quaternion to a rotation object
    rotation = tf.Rotation.from_quat(quaternion)  # [x, y, z, w]
    
    # Apply quaternion to a reference forward vector to get 3D vector
    reference_vector = rotation.apply([0, 0, 1])  # Change this if needed

    # Normalize the reference vector
    reference_vector = reference_vector / np.linalg.norm(reference_vector)
    
    return reference_vector

def compute_angle_with_quaternion(pos_a, pos_b, reference_vector):
    # Compute direction vector from A to B
    direction = pos_b - pos_a
    direction = direction / np.linalg.norm(direction)  # Normalize

    # Compute dot product
    dot_product = np.dot(direction, reference_vector)

    # Clamp to avoid floating-point errors
    dot_product = np.clip(dot_product, -1.0, 1.0)

    # Compute angle in degrees
    angle_rad = np.arccos(dot_product)
    angle_deg = np.degrees(angle_rad)

    #print(f"Got {angle_deg} from comparing {direction} with {reference_vector}")

    return angle_deg

def plot_trajectory(data, name):
    fig = plt.figure() # param figsize=(15, 5)
    ax = fig.add_subplot(projection='3d')
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
    ax.set_title(name)

    plt.tight_layout()
    plt.show()

def plot_angular_difference(data, name, horizontal=False, vertical=False, per_diver=False):
    angles = []
    horizontal_angles = []
    vertical_angles = []
    diver_data_positions = []
    divers_rescued = 0
    distance_steps = []
    time_steps = []

    for i in range(data.shape[1]):
        time_stamp = data[:, i]
        if time_stamp[-2] > divers_rescued:
            diver_data_positions.append(i)
            divers_rescued += 1
        curr_target = positions_for_plot[:, ordering[name][time_stamp[-2]]]
        curr_position = time_stamp[:3]
        euler_rotation = convert_quat_to_vector(time_stamp[5:9])
        angle = compute_angle_with_quaternion(curr_position, curr_target, euler_rotation)
        angles.append(angle)
        distance_steps.append(time_stamp[3])
        time_steps.append(time_stamp[4])

        if horizontal:
            horizontal_target = curr_target
            horizontal_target[1] = 0
            horizontal_position = curr_position
            horizontal_position[1] = 0
            horizontal_rotation = euler_rotation
            horizontal_rotation[0] = 0
            horizontal_angle = compute_angle_with_quaternion(horizontal_position, horizontal_target, horizontal_rotation)
            horizontal_angles.append(horizontal_angle)

        if vertical:
            vertical_target = curr_target
            vertical_target[0] = vertical_target[2] = 0
            vertical_position = curr_position
            vertical_position[0] = vertical_position[2] = 0
            vertical_rotation = euler_rotation
            vertical_rotation[1] = 0
            vertical_angle = compute_angle_with_quaternion(vertical_position, vertical_target, vertical_rotation)
            vertical_angles.append(vertical_angle)

    diver_data_positions.append(data.shape[1])

    if not per_diver:
        fig = plt.figure()
        ax = fig.add_subplot(311)
        ax.plot(angles)
        ax.set_title(name + " - Angles")
        ax.set_ylim([0, 180])

        if horizontal:
            ax2 = fig.add_subplot(312)
            ax2.plot(horizontal_angles)
            ax2.set_title(name + " - Horizontal angles")
            ax2.set_ylim([0, 180])

        if vertical:
            ax3 = fig.add_subplot(313)
            ax3.plot(vertical_angles)
            ax3.set_title(name + " - Vertical angles")
            ax3.set_ylim([0, 180])
    
    else:
        for i in range(divers_rescued):
            fig = plt.figure()
            ax = fig.add_subplot(311)
            ax.plot(angles[diver_data_positions[i]:diver_data_positions[i + 1]])
            ax.set_title(name + " - Angles at path " + str(i))
            ax.set_ylim([0, 180])

            if horizontal:
                ax2 = fig.add_subplot(312)
                ax2.plot(horizontal_angles[diver_data_positions[i]:diver_data_positions[i + 1]])
                ax2.set_title(name + " - Horizontal angles at path " + str(i))
                ax2.set_ylim([0, 180])

            if vertical:
                ax3 = fig.add_subplot(313)
                ax3.plot(vertical_angles[diver_data_positions[i]:diver_data_positions[i + 1]])
                ax3.set_title(name + " - Vertical angles at path " + str(i))
                ax3.set_ylim([0, 180])
            plt.tight_layout()

    plt.tight_layout()
    plt.show()

def plot_angular_difference_unity(name, data):
    file = open(name, "r")
    file.readline()
    #angular_data = [float(angle) for angle in file.readlines()]
    diver_indices = [0]
    angular_data = []
    distances = []
    counter = 0
    for time_stamp in file.readlines():
        separated_time_stamp = time_stamp.split(',')
        angular_data.append([float(separated_time_stamp[0]), float(separated_time_stamp[1]), float(separated_time_stamp[2])])
        distances.append(float(separated_time_stamp[3]))
        current_diver = int(separated_time_stamp[4])
        if current_diver > len(diver_indices):
            diver_indices.append(counter)
        counter += 1
        
    time = []
    for i in range(len(angular_data)):
        time.append(i * 0.02)
    
    angular_data = np.array(angular_data)
    angular_data = np.transpose(angular_data)

    angles = angular_data[0]
    horizontal_angles = angular_data[1]
    vertical_angles = angular_data[2]

    print(diver_indices)

    for i in range(len(diver_indices) - 1):
        print(angles[diver_indices[i]:diver_indices[i + 1]])
        fig = plt.figure()
        ax = fig.add_subplot()
        ax.plot(time[diver_indices[i]:diver_indices[i + 1]], angles[diver_indices[i]:diver_indices[i + 1]], label="Angular Difference", color='b')
        #ax.legend()
        ax.set_title(name.split('\\')[-1])
        ax.set_ylim([0, 180])

        ax_dist = ax.twinx()
        ax_dist.plot(time[diver_indices[i]:diver_indices[i + 1]], distances[diver_indices[i]:diver_indices[i + 1]], label="Distance", color='r')
        #ax2.legend()
        fig.legend()

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
csv_files = glob.glob(os.path.join(script_dir + "/full_data", "*.csv"))
processed_csv_files = glob.glob(os.path.join(script_dir + "/processed_data", "*.csv"))
txt_files = glob.glob(os.path.join(script_dir + "/processed_data", "*.txt"))

data = load_csv_to_numpy(csv_files[-1])
#plot_trajectory(data, os.path.basename(csv_files[-1]))
plot_angular_difference_unity(processed_csv_files[-1], data)
#plot_angular_difference(data, os.path.basename(csv_files[1]), horizontal=True, vertical=False, per_diver=True)
#plot_angular_difference(data, os.path.basename(csv_files[1]), horizontal=True, vertical=True, per_diver=True)

'''
for file_path in csv_files:
    data = load_csv_to_numpy(os.path.basename(file_path))
    plot_trajectory(data, os.path.basename(file_path))
    #plot_angular_difference(data, os.path.basename(file_path), per_diver=True)'
'''
