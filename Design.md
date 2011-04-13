Something about Sand Design Document
====================================

Philosophy
==========

Why create this game? Why would someone want to play it?
--------------------------------------------------------

Sand is a cooperative multiplayer game which only works with people in your immediate vicinity, so the primary reason one might want to play it is to have fun with their friends. In addition, it has elements of a real-time strategy game: there is a good amount of strategy to tool and weapon choice as well as actions one takes once on the playing field. Unlike most RTSes, however, it gives each player control of only a single unit.

What is the immediate and long-term projected socio-cultural impact of this project?
------------------------------------------------------------------------------------

Each player in Sand is given the ability to drastically customize the attributes of their character, but such customization is entirely targeted at expanding the player's abilities for the benefit of the entire team, instead of for the benefit of that particular player. Indeed, it will be impossible to play Sand as a "maverick" - the whole team *must* work together to win. So, the development of communication skills and the destruction of selfish attitudes are two potential socio-cultural impacts of this project.

Are there any previous games in this genre?
-------------------------------------------

There are currently existing games which provide one part or another of our game: Valve's Alien Swarm, for example, provides the classic top-down multi-class portion, Geometry Wars touches somewhat on the visual style of the game, and so on.

What is this game's target audience?
------------------------------------

This game's target audience is primarily fans of competitive team gameplay. It will likely primarily interest gamers, not casual players, as the level of potential customization is too great and the number of people required to play is too high for someone less experienced with games.

Common Questions
================

What is the game?
-----------------

It’s a 3v3 abstract top-down map-conquest game!

The game is broken up into two alternating phases. The goal in phase 1 is to get your team’s sand onto the playing field and spread it out. A slider on the screen will move left or right depending on which team is winning and how close they are to winning phase 1. When the slider goes all the way to one end or the other, the round enters phase 2. All Primary tools are disabled; sand, water, and grease remain in place. The goal in phase 2 is to have the entire enemy team shocked at the same time. Phase 2 is timed.

* If the team that won phase 1 also wins phase 2, they score a point.
* If the team that lost phase 1 wins phase 2, no one scores a point.
* If the time limit runs out before either team wins phase 2, no one scores a point.

In any of these three cases, all sand, water, and grease disappears and the next round immediately starts. Shock effects on the losing team remain, giving the winning team a head start on the next round. Games are played to a certain number of points.

Where does the game take place?
-------------------------------

The game takes place on an two-dimensional abstract world with fixed obstacles.

What do I control?
------------------

You control a tank, with a set of tools. Tools are described in greater detail in the character section.

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

The Game World
==============

Overview
--------

The game world consists solely of ground and barrier walls. There are multiple loadable maps, providing for a dynamic gaming experience.

Rendering System
----------------

Sand uses Microsoft's XNA libraries for both graphics and networking. These provide hardware-accelerated access to the graphics card through DirectX, allowing rapid rendering of our game world.

Camera
------

The camera is top-down, aimed towards a two-dimensional surface. It is fixed during gameplay, giving all players a complete view of the game world.

Characters
==========

Overview
--------

Creating a Character
--------------------

During the beginning of a game, each player is asked to choose their team, class, and loadout. The loadout selection involves choosing from a wide variety of tools, weapons, and powerups, detailed below.

Primaries
---------

*Defense*

* Jet - Creates sand in front of the Defensive tank.
* Sand Charge - Creates sand all around the Defensive tank.
* Sand Grenade - Bounces off walls and explodes after a set time, filling the blast radius with sand.
* Water Grenade - Bounces off walls and explodes after a set time, filling the blast radius with water.
 
*Offense*

* Laser - Destroys sand in a small radius around the crosshair. Since it has sustained fire, it will immediately burn sand underneath water after evaporating the water.
* Flame Charge - Destroys sand all around the Offensive tank.
* Flame Grenade - Bounces off walls and explodes after a set time, burning sand in the blast radius.
* Grease Grenade - Bounces off walls and explodes after a set time, filling the blast radius with grease.
 
*Support*

* Plow - Pushes sand in front of the Support tank. Good for making piles.
* Pressure Charge - Pushes sand outward, all around the Support tank. Good for spreading out.
* Nullifier - Destroys water and grease around the Support tank.
* Duplicator - Assumes the form of a nearby teammate’s currently selected Primary tool. The Duplicator has no inherent energy consumption, but the duplicated Primary tool does. All duplicated tools assume the Modifier position set for the Duplicator.

Weapons
-------

* Cannon - Shocks one target in front of the tank.
* EMP - Shocks all targets near the tank.
* Shock Grenade - Bounces off walls and explodes after a set time, shocking targets in the blast radius. Careful, you can shock yourself with this weapon.
 
Utilities
---------

* Shield - Blocks incoming shock attacks.
* Prism - Blocks an incoming shock and converts it to a single Cannon attack, which can be fired by hitting the Prism key again. Energy is fully depleted when an attack is absorbed and does not recharge until the Cannon attack is used.
* Barrier Ray - Creates walls along the player’s moving crosshair, which can be destroyed by shock attacks.
* Grapple - Links the player’s tank to any other tank, restricting the movement of both tanks according to the rope length. Good for trapping enemies or pulling immobilized teammates out of danger. The rope breaks if it touches any wall.
* Ground - Ends a shock effect on another player.
* Overcharge - Boosts your Voltage until your next weapon attack. Voltage increases gradually after Overcharge is activated. Energy does not recharge while Overcharge is active.

Mobilities
----------

* Boost Drive - Temporary speed boost.
* Blink Drive - Teleports to the crosshair.
* Wink Drive - Temporary invisibility. Cannot use other tools while invisible.

User Interface
==============

Overview
--------

The user interface during gameplay is very simple: the map takes up the majority of the screen, while the 

User Interface Detail x
-----------------------

Sound
=====

Overview
--------

There are a variety of abstract sounds in the game, designed to fit in with the visual style and provide easy action cues to all players.

Multiplayer Sound
-----------------

Sounds generated by each player are rebroadcast to all others, and played with positional and volume cues. This gives each player heightened awareness of what's going on immediately around them in the game space.

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
