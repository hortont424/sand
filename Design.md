Something about Sand Design Document
====================================

Philosophy
==========

Why create this game? Why would someone want to play it?
----------------------------------------------

Sand is a cooperative multiplayer game which only works with people in your immediate vicinity, so the primary reason one might want to play it is to have fun with their friends. In addition, it has elements of a real-time strategy game: there is a good amount of strategy to tool and weapon choice as well as actions one takes once on the playing field. Unlike most RTSes, however, it gives each player control of only a single unit.

What is the immediate and long-term projected socio-cultural impact of this project?
--------------------------------------------------

Each player in Sand is given the ability to drastically customize the attributes of their character, but such customization is entirely targeted at expanding the player's abilities for the benefit of the entire team, instead of for the benefit of that particular player. Indeed, it will be impossible to play Sand as a "maverick" - the whole team *must* work together to win. So, the development of communication skills and the destruction of selfish attitudes are two potential socio-cultural impacts of this project.

Are there any previous games in this genre?
------------------------------

There are currently existing games which provide one part or another of our game: Valve's Alien Swarm, for example, provides the classic top-down multi-class portion, Geometry Wars touches somewhat on the visual style of the game, and so on.

What is this game's target audience?
------------------------

This game's target audience is primarily fans of competitive team gameplay. It will likely primarily interest gamers, not casual players, as the level of potential customization is too great and the number of people required to play is too high for someone less experienced with games.

Common Questions
================

What is the game?
-----------------

It’s a 3v3 abstract top-down map-conquest game!

Where does the game take place?
-------------------------------

The game takes place on an abstract world with fixed obstacles.

What do I control?
------------------

You control a tank, with a set of tools.

How many characters do I control?
---------------------------------

Each player controls one character at a time.

What is the main focus?
-----------------------

Filling the map with sand, and then (based on the outcome of the sand battle, winners will be offense, losers will be defense) stunning the other team.

What's different?
-----------------

It's a 3rd-person shooter with no death, instead primarily focusing on the creation and destruction of inanimate particles of sand.

Features
========

General Features
----------------

* Top-down view of the game world
* Vector style

Multiplayer Features
--------------------

* 3v3 multiplayer
* LAN only
* Zero-configuration game setup

Gameplay
--------

The Game World
==============

Overview
--------

World Feature x
---------------

The Physical World
------------------

Rendering System
----------------

Sand uses Microsoft's XNA libraries for both graphics and networking. These provide hardware-accelerated access to the graphics card through DirectX, allowing rapid rendering of our game world.

Camera
------

The camera is top-down, aimed towards a two-dimensional surface. <does it move??>

World Layout
------------

Characters
==========

Overview
--------

Creating a Character
--------------------

Enemies and Monsters
--------------------

User Interface
==============

Overview
--------

User Interface Detail x
-----------------------

Sound
=====

Overview
--------

Red Book Audio
--------------

3D Sound
--------

Sound Design
------------

Multiplayer
===========

Overview
--------

Max Players
-----------

The maximum number of players in a single game of Sand is 6, as each game is comprised of two teams of three players each.

Servers
-------

The first player to launch the game on a particular network becomes the de-facto server. <this might change>

Customization
-------------

Before the game begins, each player has the opportunity to select his loadout (there are five slots: two primaries, a weapon, utility, and mobility modifier), and many of the tools can be modified with a single slider which increases a positive and a negative attribute of the player in a balanced fashion.

Internet
--------

This game will not work over the internet. For simplicity of implementation, it will be limited to LAN games.

Persistence
-----------

The world is not persistent. Each match will use either a predefined or procedurally generated level which will persist only for the duration of the match.

Saving and Loading
------------------

Players will not be able to save or load multiplayer games; matches are relatively short; as such, being able to do so makes little sense.

Character Rendering
===================

Overview
--------

Characters are static icons which are guided around the screen by their players. Each class has a different icon, and each team is a different color. The icons have a well-defined direction to make it relatively easy to tell in which direction they're pointing, and each one is an abstract representations of its class' primary goal.

Character Rendering Detail x
----------------------------
