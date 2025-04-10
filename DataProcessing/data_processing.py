import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import os
import glob
from mpl_toolkits.mplot3d.art3d import Line3DCollection
import matplotlib.cm as cm
import matplotlib.colors as mcolors
import matplotlib as mpl
from matplotlib.lines import Line2D

mpl.rcParams['font.family'] = 'serif'

naming = {}
naming["11-02-2025_10-24-56_processed.csv"] = "VIP_1" #"Visually impaired player 1"
naming["11-02-2025_10-24-56.csv"] = "VIP_1" #"Visually impaired player 1"

naming["11-02-2025_11-43-22_processed.csv"] = "VIP_2" #"Visually impaired player 2"
naming["11-02-2025_11-43-22.csv"] = "VIP_2" #"Visually impaired player 2"

naming["11-02-2025_13-52-54_processed.csv"] = "VIP_3" #"Visually impaired player 3"
naming["11-02-2025_13-52-54.csv"] = "VIP_3" #"Visually impaired player 3"

naming["16-12-2024_14-07-40_processed.csv"] = "VIP_4" #"Visually impaired player 4"
naming["16-12-2024_14-07-40.csv"] = "VIP_4" #"Visually impaired player 4"

naming["19-12-2024_11-54-36_processed.csv"] = "VIP_5" #"Visually impaired player 5"
naming["19-12-2024_11-54-36.csv"] = "VIP_5" #"Visually impaired player 5"

naming["23-03-2025_21-33-31_processed.csv"] = "SP_1" #"Sighted player 1"
naming["23-03-2025_21-33-31.csv"] = "SP_1" #"Sighted player 1"

naming["25-03-2025_19-17-19_processed.csv"] = "SP_2" #"Sighted player 2"
naming["25-03-2025_19-17-19.csv"] = "SP_2" #"Sighted player 2"

naming["27-03-2025_10-15-21_processed.csv"] = "SP_3" #"Sighted player 3"
naming["27-03-2025_10-15-21.csv"] = "SP_3" #"Sighted player 3"

naming["28-03-2025_13-08-25_processed.csv"] = "SP_4_NS" #"Sighted player 4 (no screen)"
naming["28-03-2025_13-08-25.csv"] = "SP_4_NS" #"Sighted player 4 (no screen)"

naming["03-04-2025_17-02-37_processed.csv"] = "SP_5_NS" #"Sighted player 5 (no screen)"
naming["03-04-2025_17-02-37.csv"] = "SP_5_NS" #"Sighted player 5 (no screen)"

naming["03-04-2025_17-27-39_processed.csv"] = "SP_5" #"Sighted player 5"
naming["03-04-2025_17-27-39.csv"] = "SP_5" #"Sighted player 5"

ordering = {}
ordering["11-02-2025_10-24-56.csv"] = [0, 1, 5, 6]
ordering["11-02-2025_11-43-22.csv"] = [0, 1, 5, 2, 6] #015 6 2
ordering["11-02-2025_13-52-54.csv"] = [0, 1, 2, 6]
ordering["16-12-2024_14-07-40.csv"] = [0, 1, 6]
ordering["19-12-2024_11-54-36.csv"] = [0, 1, 3, 2, 6]
ordering["23-03-2025_21-33-31.csv"] = [0, 1, 5, 3, 2, 4, 6] #01532 6 46
ordering["25-03-2025_19-17-19.csv"] = [0, 1, 5, 2, 4, 3, 6]
ordering["27-03-2025_10-15-21.csv"] = [0, 5, 1, 4, 2, 3, 6] #05142 6 36
ordering["28-03-2025_13-08-25.csv"] = [0, 1, 5, 2, 3, 6]
ordering["03-04-2025_17-02-37.csv"] = [ 0, 2, 1, 6],
ordering["03-04-2025_17-27-39.csv"] = [ 0, 1, 5, 2, 4, 3, 6]

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

### Processed data:
# Angle
# Horizontal angle
# Vertical angle
# Distance to diver
# Divers saved

### Data:
# Position X, Y, Z
# Distance traveled
# Time
# Submarine Rotation X, Y, Z, W
# Camera Rotation X, Y, Z, W
# Main Light On, Left Light On, Right Light On
# Saved Divers
# Electricity

def load_csv_to_numpy(file_path):
    df = pd.read_csv(file_path, low_memory=False)
    data_array = df.to_numpy()
    return data_array.transpose()

def calculate_percentage(value, num_values, decimals=2):
    value /= num_values
    value *= 100
    value = round(value, decimals)
    return value

def calculate_per_second(value, num_values, values_gathered_per_second=50, decimals=2):
    value /= (num_values / values_gathered_per_second)
    value = round(value, decimals)
    return value

def plot_trajectory(data, name):
    fig = plt.figure() # param figsize=(15, 5)
    fig.set_size_inches(50, 30)
    ax = fig.add_subplot(121, projection='3d')
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
    line = Line3DCollection(segments, colors=colors, linewidth=3.5)
    ax.add_collection3d(line)  # Add trajectory to plot

    ax.plot3D(data[0], data[2], 0, color='grey', linewidth=2, alpha=0.4, label='Shaded trajectory')
    ax.scatter3D(diver_positions[0], diver_positions[2], diver_positions[1], color='red', label='Divers', s=100)
    ax.scatter3D(base_position[0], base_position[2], base_position[1], color='saddlebrown', label='Base', s=150)
    ax.scatter3D(positions_for_plot[0], positions_for_plot[2], 0, color='grey', alpha=0.5)
    #ax.set_title(naming[name.split('\\')[-1]] + " - Trajectory")
    ax.view_init(elev=45, azim=15)

    ax.set_xlabel('X Position (m)', labelpad=35)
    ax.set_ylabel('Z Position (m)', labelpad=30)
    ax.set_zlabel('Y Position (m)', labelpad=30)
    ax.tick_params(pad=10)

    distances = np.array(data[4], dtype=float)
    norm = mcolors.Normalize(vmin=np.min(distances), vmax=np.max(distances))
    colors = cmap(norm(distances[:-1]))
    mappable = cm.ScalarMappable(norm=norm, cmap=cmap)
    mappable.set_array(distances)  # This will now work
    fig.colorbar(mappable, ax=ax, shrink=0.5, aspect=15, pad=0.1, label='Distance (m)')
    
    mid_val = (np.max(distances) + np.min(distances)) / 2
    mid_color = cmap(norm(mid_val))
    trajectory_legend = Line2D([0], [0], color=mid_color, linewidth=3.5, label='Trajectory')
    handles, labels = ax.get_legend_handles_labels()
    handles.insert(0, trajectory_legend)
    labels.insert(0, "Trajectory")

    ax.legend(handles, labels, loc='upper right', bbox_to_anchor=(1.1, 1))
    #plt.legend()

    plt.tight_layout()
    plt.savefig(naming[name.split('\\')[-1]] + "_T.png", bbox_inches='tight')
    plt.show()

def plot_angular_difference_unity(name):
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
        if current_diver > len(diver_indices) - 1:
            diver_indices.append(counter)
        counter += 1
    
    diver_indices.append(counter)
        
    time = []
    for i in range(len(angular_data)):
        time.append(i * 0.02)
    
    angular_data = np.array(angular_data)
    angular_data = np.transpose(angular_data)

    angles = angular_data[0]
    horizontal_angles = angular_data[1]
    vertical_angles = angular_data[2]

    plt.rcParams.update({'font.size': 34})

    for i in range(len(diver_indices) - 1):
        fig = plt.figure()
        fig.set_size_inches(60, 14)
        #fig.suptitle(naming[name.split('\\')[-1]] + " - Path " + str(i + 1))

        ax = fig.add_subplot(131)
        ax.plot(time[0:diver_indices[i + 1] - diver_indices[i]], angles[diver_indices[i]:diver_indices[i + 1]], label="Angular Difference", color='b', linewidth=2)
        ax.set_title("Angular difference")
        ax.set_ylim([0, 180])
        ax.set_xlabel('Time (s)')
        ax.set_ylabel('Angular difference (°)')

        ax_dist = ax.twinx()
        ax_dist.plot(time[0:diver_indices[i + 1] - diver_indices[i]], distances[diver_indices[i]:diver_indices[i + 1]], label="Distance", color='r', linewidth=2)
        ax_dist.set_ylabel('Distance (m)')

        ax_hori = fig.add_subplot(132)
        ax_hori.plot(time[0:diver_indices[i + 1] - diver_indices[i]], horizontal_angles[diver_indices[i]:diver_indices[i + 1]], color='b', linewidth=2)
        ax_hori.set_title("Horizontal angular difference")
        ax_hori.set_ylim([0, 180])
        ax_hori.set_xlabel('Time (s)')
        ax_hori.set_ylabel('Angular difference (°)')

        ax_dist_hori = ax_hori.twinx()
        ax_dist_hori.plot(time[0:diver_indices[i + 1] - diver_indices[i]], distances[diver_indices[i]:diver_indices[i + 1]], color='r', linewidth=2)
        ax_dist_hori.set_ylabel('Distance (m)')

        ax_vert = fig.add_subplot(133)
        ax_vert.plot(time[0:diver_indices[i + 1] - diver_indices[i]], vertical_angles[diver_indices[i]:diver_indices[i + 1]], color='b', linewidth=2)
        ax_vert.set_title("Vertical angular difference")
        ax_vert.set_ylim([0, 180])
        ax_vert.set_xlabel('Time (s)')
        ax_vert.set_ylabel('Angular difference (°)')

        ax_dist_vert = ax_vert.twinx()
        ax_dist_vert.plot(time[0:diver_indices[i + 1] - diver_indices[i]], distances[diver_indices[i]:diver_indices[i + 1]], color='r', linewidth=2)
        ax_dist_vert.set_ylabel('Distance (m)')

        fig.legend(loc='lower center', bbox_to_anchor=(0.5, -0.05), ncol=2)
        #plt.savefig(naming[name.split('\\')[-1]] + "_" + str(i + 1) + ".png", bbox_inches='tight')

def calculate_values(name, data, processed_data):
    data = np.transpose(data)
    processed_data = np.transpose(processed_data)

    print(name)
    
    ### Value variables
    facing_target_percentage = 0
    facing_target_horizontally_percentage = 0
    facing_target_vertically_percentage = 0
    moving_towards_target_percentage = 0
    standing_still_percentage = 0
    backing_towards_target_percentage = 0
    average_approach_per_second = 0
    
    ### Helper variables
    last_distance_to_diver = processed_data[0][3]
    last_divers_saved = processed_data[0][4]

    for angle, horizontal_angle, vertical_angle, distance_to_diver, divers_saved in processed_data:
        ### Percentage of time facing target
        if angle <= 90.0:
            facing_target_percentage += 1
        if horizontal_angle <= 90.0:
            facing_target_horizontally_percentage += 1
        if vertical_angle <= 90.0:
            facing_target_vertically_percentage += 1
        
        ### Percentage of time getting closer to target
        if distance_to_diver < last_distance_to_diver:
            moving_towards_target_percentage += 1
            if (angle > 90):
                backing_towards_target_percentage += 1
        
        ### Percentage of time standing still
        if distance_to_diver - last_distance_to_diver == 0:
            standing_still_percentage += 1

        ### Average speed of coming to the target per second
        if last_divers_saved == divers_saved:
            average_approach_per_second += last_distance_to_diver - distance_to_diver
        
        ### Update helper variables
        last_distance_to_diver = distance_to_diver
        last_divers_saved = divers_saved

    angle_mean = round(np.mean(processed_data[:, 0]), 2)
    angle_std = round(np.std(processed_data[:, 0]), 2)
    print(f"Angle mean: {angle_mean}°, Angle std: {angle_std}°")

    horizontal_angle_mean = round(np.mean(processed_data[:, 1]), 2)
    horizontal_angle_std = round(np.std(processed_data[:, 1]), 2)
    print(f"    Horizontal angle mean: {horizontal_angle_mean}°, Horizontal angle std: {horizontal_angle_std}°")

    vertical_angle_mean = round(np.mean(processed_data[:, 2]), 2)
    vertical_angle_std = round(np.std(processed_data[:, 2]), 2)
    print(f"    Vertical angle mean: {vertical_angle_mean}°, Vertical angle std: {vertical_angle_std}°")
    
    facing_target_percentage = calculate_percentage(facing_target_percentage, processed_data.shape[0])
    print(f"Facing target percentage: {facing_target_percentage} %")

    facing_target_horizontally_percentage = calculate_percentage(facing_target_horizontally_percentage, processed_data.shape[0])
    print(f"    Facing target horizontally percentage: {facing_target_horizontally_percentage} %")

    facing_target_vertically_percentage = calculate_percentage(facing_target_vertically_percentage, processed_data.shape[0])
    print(f"    Facing target vertically percentage: {facing_target_vertically_percentage} %")

    moving_towards_target_percentage = calculate_percentage(moving_towards_target_percentage, processed_data.shape[0])
    print(f"Moving towards target percentage: {moving_towards_target_percentage} %")

    backing_towards_target_percentage = calculate_percentage(backing_towards_target_percentage, processed_data.shape[0])
    print(f"    Backing towards target percentage: {backing_towards_target_percentage} %, relative to moving towards target: {round(backing_towards_target_percentage / moving_towards_target_percentage * 100, 2)} %")

    standing_still_percentage = calculate_percentage(standing_still_percentage, processed_data.shape[0])
    print(f"Standing still percentage: {standing_still_percentage} %")

    average_approach_per_second = calculate_per_second(average_approach_per_second, processed_data.shape[0], decimals=3)
    print(f"Average approach per second: {average_approach_per_second}")

    print("\n")

csv_files = glob.glob(os.path.join(script_dir + "/full_data", "*.csv"))
processed_csv_files = glob.glob(os.path.join(script_dir + "/processed_data", "*.csv"))
txt_files = glob.glob(os.path.join(script_dir + "/processed_data", "*.txt"))

plt.rcParams.update({'font.size': 28})

#data = load_csv_to_numpy(csv_files[1])
#plot_trajectory(data, os.path.basename(csv_files[1]))
#plot_angular_difference_unity(processed_csv_files[1])

counter = 0
for file_path in csv_files:
    name = naming[file_path.split('\\')[-1]]
    data = load_csv_to_numpy(file_path)
    data = data[:,1:]
    processed_data = load_csv_to_numpy(processed_csv_files[counter])

    #plot_trajectory(data, file_path)
    #plot_angular_difference_unity(processed_csv_files[counter])

    calculate_values(name, data, processed_data)

    counter += 1
