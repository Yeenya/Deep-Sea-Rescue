import pandas as pd
import matplotlib as mpl
import matplotlib.pyplot as plt

mpl.rcParams['font.family'] = 'serif'

# Load the CSV
df = pd.read_csv("feedback.csv")

# Set the respondent type column name (assumed to be the first)
type_col = df.columns[0]
question_cols = df.columns[1:]

# Unique respondent types
types = df[type_col].unique()

colors = {
    "Visually impaired player": "#b04c44",
    "Sighted player": "#88a15b",
    "Sighted player (monitor off)": "#4b7ba3"
}

averages = {t: [] for t in types}

for question in question_cols:
    used_rating_range = range(1, 6) if question == "6" else range(1, 7) 
    # Prepare data: counts per type per rating
    stacked_data = {t: [] for t in types}
    for rating in used_rating_range:
        for t in types:
            count = df[(df[type_col] == t) & (df[question] == rating)].shape[0]
            stacked_data[t].append(count)

    # Plot
    bottom = [0] * len(used_rating_range)
    plt.figure(figsize=(8, 5))
    for t in types:
        plt.bar(used_rating_range, stacked_data[t], label=t, bottom=bottom, color=colors[t])
        bottom = [sum(x) for x in zip(bottom, stacked_data[t])]

    plt.xlabel("Rating")
    plt.ylabel("Number of responses")
    plt.ylim(0, 9)
    plt.xticks(used_rating_range)
    plt.legend()
    plt.tight_layout()
    plt.savefig(f"Q{question}.png", dpi=300)
    plt.show()

    # Calculate averages per type for this question
    for t in types:
        ratings = df[df[type_col] == t][question].dropna()
        avg = ratings.mean() if not ratings.empty else None
        averages[t].append(round(avg, 2))

# Create LaTeX table grouped by question
print("\\begin{table}[h!]")
print("\\centering")
print("\\begin{tabular}{l" + "c" * len(types) + "}")
print("\\toprule")
print("\\textbf{Question} & " + " & ".join(f"\\textbf{{{t}}}" for t in types) + " \\\\")
print("\\midrule")

for i, q in enumerate(question_cols):
    row = [f"{averages[t][i]:.2f}" if averages[t][i] is not None else "–" for t in types]
    print(f"{q} & " + " & ".join(row) + " \\\\")

print("\\bottomrule")
print("\\end{tabular}")
print("\\caption{Average ratings per question by respondent type}")
print("\\label{tab:avg_ratings}")
print("\\end{table}")