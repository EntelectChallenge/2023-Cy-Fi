import sys, os
domain = os.path.dirname(sys.path[0])
sys.path.append(os.path.abspath(os.path.join(domain, "..", "bin", "Debug", "net6.0")))

from pythonnet import load
load("coreclr")
import clr
clr.AddReference(os.path.abspath(os.path.join(domain, "..", "bin", "Debug", "net6.0", "Domain.dll")))

import Domain.Objects
from Domain.Objects import WorldObject

import matplotlib.pyplot as plt
import matplotlib.gridspec as gridspec

from matplotlib.colors import ListedColormap
from matplotlib.widgets import Slider, TextBox, CheckButtons, RangeSlider
import matplotlib.ticker as ticker

import numpy as np

fig = plt.figure()
gs = gridspec.GridSpec(10,2, figure=fig)

width = 500
height = 200

mapPlot = fig.add_subplot(gs[0:, 0])

'''worldGenerator = WorldObject(width, 
height, 
'seed', 
50, 
0.025, 
0.115, 
0.065, # Min Height
0.13,  # Max Height
0.005, 
1, 
6, 
9)'''

worldGenerator = WorldObject(
	width, 
	height, 
	493048330493, 
	50, 
	0.025, 
	0.115,  
	0.065, # Min Height
	0.13,  # Max Height
	0.005, 
	1, 
	6, 
	9)

worldMap = np.array(worldGenerator.map)

mapPlot.set_xlim(0, width)
mapPlot.set_ylim(0, height)
paths = []

linewidth = 450*pow(width, -0.999)

for idx, path in enumerate(worldGenerator.randomPaths):
	path_x = []
	path_y = []
	for coord in path:
		path_x.append(coord.Item1 + 0.5)
		path_y.append(coord.Item2 + 0.5)

	paths.append(mapPlot.plot(path_x, path_y, lw=linewidth, label=f'Path {idx}'))

mapPlot.imshow(worldMap.T, extent=(0, width, 0, height), origin="lower", cmap=ListedColormap(["white", "saddlebrown", "blue", "red", "black", "green"]), interpolation="none")

#=== === === === === === === ===	World Generation	=== === === === === === === ===
def generate(value):

	#newWorld = WorldObject(width_slider.val, height_slider.val, seed_input.text, fill_slider.val, path_width_slider.val[0]/100, path_width_slider.val[1]/100, path_height_slider.val[0]/100, path_height_slider.val[1]/100, path_cleanup_width_slider.val/100, lod_slider.val, connections_slider.val[0], connections_slider.val[1])
	
	newWorld = WorldObject(
		width_slider.val, 
		height_slider.val, 
		seed_input.text, 
		fill_slider.val, 
		path_width_slider.val[0]/100, 
		path_width_slider.val[1]/100, 
		 0.032, # Min height
		 0.2, # Max height
		path_cleanup_width_slider.val/100, 
		lod_slider.val, 
		connections_slider.val[0], 
		connections_slider.val[1])
	
	

	mapPlot.cla()

	linewidth = 450*pow(width_slider.val, -0.999)
	
	for idx, path in enumerate(newWorld.randomPaths):
		path_x = []
		path_y = []
		for coord in path:
			path_x.append(coord.Item1 + 0.5)
			path_y.append(coord.Item2 + 0.5)

		p_plot = mapPlot.plot(path_x, path_y, lw=linewidth, label=f'Path {idx}')
		p_plot[0].set_visible(not show_paths.get_status()[0])
		paths.append(p_plot)


#	for x, y in enumerate(newWorld.map):
#		if newWorld.map[x][1] == 3:
#			#x.append(coord.Item1 + 0.5)
#			#y.append(coord.Item2 + 0.5)
#
#			p_plot = mapPlot.plot(x, 1, lw=conewidth, label=f'Collectable {idx}')
#			#p_plot[3].set_visible(not show_paths.get_status()[3])
#			paths.append(p_plot)

		
	#mapPlot.legend(loc='upper left')
	mapPlot.set_xlim(0, width_slider.val);
	mapPlot.set_ylim(0, height_slider.val);
	
	mapPlot.imshow(np.array(newWorld.map).T, extent=(0, width_slider.val, 0, height_slider.val), origin="lower", cmap=ListedColormap(["white", "saddlebrown", "blue", "black","red"]), interpolation="none")
	
#=== === === === === === === ===	Web page Elements	=== === === === === === === ===

# === === ===		Sliders					===	=== ===
width_slider = Slider(ax=fig.add_subplot(gs[0,1]), label='Width', valmin=1, valmax=10000, valinit=width, valstep=1)
width_slider.on_changed(generate)

height_slider = Slider(ax=fig.add_subplot(gs[1,1]), label='Height', valmin=1, valmax=10000, valinit=height, valstep=1)
height_slider.on_changed(generate)

fill_slider = Slider(ax=fig.add_subplot(gs[3,1]), label='Fill percentage', valmin=1, valmax=100, valinit=0, valstep=1)
fill_slider.on_changed(generate)

path_cleanup_width_slider = Slider(ax=fig.add_subplot(gs[4,1]), label='Path cleanup width percentage', valmin=0, valmax=100, valinit=1, valstep=0.5)
path_cleanup_width_slider.on_changed(generate)

lod_slider = Slider(ax=fig.add_subplot(gs[7,1]), label='Level', valmin=0, valmax=3, valinit=0, valstep=1)
lod_slider.on_changed(generate)

# === === ===		Text Box				===	=== ===
seed_input = TextBox(ax=fig.add_subplot(gs[2,1]), label='Seed', initial='seed')
seed_input.on_text_change(generate)


# === === ===		Range Sliders			===	=== ===
path_width_slider = RangeSlider(ax=fig.add_subplot(gs[5,1]), label='Path width percentage', valmin=0.5, valmax=100, valinit=(2.5,11.5), valstep=0.5)
path_width_slider.on_changed(generate)

path_height_slider = RangeSlider(ax=fig.add_subplot(gs[6,1]), label='Path height percentage', valmin=0.5, valmax=100, valinit=(6.5,13.5), valstep=0.5)
path_height_slider.on_changed(generate)

connections_slider = RangeSlider(ax=fig.add_subplot(gs[8, 1]), label='Number of connecting paths', valmin=1, valmax=20, valinit=(6, 9), valstep=1)
connections_slider.on_changed(generate)

# === === ===		Show paths button		===	=== ===

def show_hide_paths():
	for p in paths:
		p[0].set_visible(not show_paths.get_status()[0])
	plt.draw()

show_paths = CheckButtons(ax=fig.add_subplot(gs[9,1]), labels=['Hide paths'], actives=[True])
show_paths.on_clicked(show_hide_paths)
show_hide_paths()

mng = plt.get_current_fig_manager()
mng.window.state('zoomed')
#mapPlot.legend(loc='upper left')

plt.show()
