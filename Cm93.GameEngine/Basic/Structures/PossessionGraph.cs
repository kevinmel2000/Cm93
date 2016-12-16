﻿/*
        Copyright © Iain McDonald 2013-2016
        This file is part of Cm93.

        Cm93 is free software: you can redistribute it and/or modify
        it under the terms of the GNU General Public License as published by
        the Free Software Foundation, either version 3 of the License, or
        (at your option) any later version.

        Cm93 is distributed in the hope that it will be useful,
        but WITHOUT ANY WARRANTY; without even the implied warranty of
        MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
        GNU General Public License for more details.

        You should have received a copy of the GNU General Public License
        along with Cm93. If not, see <http://www.gnu.org/licenses/>.
*/
using Cm93.Model.Helpers;
using Cm93.Model.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cm93.GameEngine.Basic.Structures
{
	public class PossessionGraph<T> where T : Player
	{
		public struct Edge<T>
		{
			public Edge(T vertex, double cost)
			{
				Vertex = vertex;
				Cost = cost;
			}

			public T Vertex;
			public double Cost;
		}

		private IList<T> Team { get; set; }
		private IList<T> Opposition { get; set; }

		private bool IsHome { get; set; }
		private bool IsDefendingZero { get; set; }

		private TeamFormationAttributes TeamFormationAttributes { get; set; }
		private Dictionary<T, IList<Edge<T>>> EdgeIndices { get; set; }
		private Random Random { get; set; }

		public PossessionGraph(TeamFormationAttributes teamFormationAttributes, bool isHome, bool isDefendingZero)
		{
			TeamFormationAttributes = teamFormationAttributes;
			EdgeIndices = new Dictionary<T, IList<Edge<T>>>();
			Random = new Random((int) DateTime.Now.Ticks);

			IsHome = isHome;
			IsDefendingZero = isDefendingZero;

			Team = IsHome ? TeamFormationAttributes.HomeTeamPlayers.Cast<T>().ToList() : TeamFormationAttributes.AwayTeamPlayers.Cast<T>().ToList();
			Opposition = IsHome ? TeamFormationAttributes.AwayTeamPlayers.Cast<T>().ToList() : TeamFormationAttributes.HomeTeamPlayers.Cast<T>().ToList();

			var orderedPlayers = IsDefendingZero ?
				Team.OrderBy(p => p.Location.Y).ToList() :
				Team.OrderByDescending(p => p.Location.Y).ToList();

			Team.
				Select((p, i) => new { Index = i, Player = p }).
				Execute(a => EdgeIndices[a.Player] = orderedPlayers.
					Skip(a.Index + 1).
					Where(op => IsDefendingZero ? op.Location.Y > a.Player.Location.Y : op.Location.Y < a.Player.Location.Y).
					Select(p => new Edge<T>(p, Cost(a.Player, p, TeamFormationAttributes.TeamStrength))).
					ToList()
				);
		}

		private double Cost(T from, T to, Func<bool, Coordinate, double> teamStrength)
		{
			// cost for distance
			var distance = Math.Sqrt(
				(from.Location.X - to.Location.X) * (from.Location.X - to.Location.X) +
				(from.Location.Y - to.Location.Y) * (from.Location.Y - to.Location.Y)
			);

			// penalty for opposing players in the way (near either player)
			var theta = Math.Atan(
				(from.Location.X - to.Location.X) /
				(from.Location.Y - to.Location.Y)
			);

			var xPrime = Math.Abs(0.1d * Math.Sin(theta));
			var yPrime = Math.Abs(0.1d * Math.Cos(theta));

			var sendX = 0d;
			var sendY = 0d;
			var receiveX = 0d;
			var receiveY = 0d;
			if (from.Location.X > to.Location.X)
			{
				sendX = from.Location.X - xPrime;
				receiveX = to.Location.X + xPrime;
			}
			else
			{
				sendX = from.Location.X + xPrime;
				receiveX = to.Location.X - xPrime;
			}

			if (from.Location.Y > to.Location.Y)
			{
				sendY = from.Location.Y - yPrime;
				receiveY = to.Location.Y + yPrime;
			}
			else
			{
				sendY = from.Location.Y + yPrime;
				receiveY = to.Location.Y - yPrime;
			}

			var receive = new Coordinate { X = receiveX, Y = receiveY };
			var send = new Coordinate { X = sendX, Y = sendY };

			var successfulPass = (teamStrength(IsHome, receive) + teamStrength(IsHome, send)) / 2;

			// skill of both passing and receiving player
			var playerSkills = (from.Rating + to.Rating) / 2;

			return (playerSkills * successfulPass) / distance;
		}

		public int PhaseOfPlay(ref T possessor, out bool isShooting)
		{
			var option = Int32.MinValue;
			var receiver = default(T);

			if (EdgeIndices[possessor].Any())
			{
				var phaseEdge = EdgeIndices[possessor].
					Select(e => new Edge<T>(e.Vertex, e.Cost + Random.Next(-500, 500))).
					Aggregate((a, b) => a.Cost > b.Cost ? a : b);

				option = (int) (
					(
						phaseEdge.Cost *
						Math.Pow(TeamFormationAttributes.TeamPositionalBalance(IsHome) / TeamFormationAttributes.TeamPositionalBalance(!IsHome), 2) *
						TeamFormationAttributes.TeamAttackingShape(IsHome)
					) /
					TeamFormationAttributes.TeamDefendingShape(!IsHome)
				);

				receiver = phaseEdge.Vertex;
			}

			var shootOption = (int) (possessor.Rating /
				(
					Math.Pow(possessor.Location.Distance(new Coordinate { X = 0.5, Y = IsDefendingZero ? 1 : 0 }), 2) *
					TeamFormationAttributes.TeamStrength(IsHome, possessor.Location) *
					TeamFormationAttributes.TeamDefendingShape(!IsHome)
				)
			);

			if (shootOption > option)
			{
				isShooting = true;
				option = shootOption;
			}
			else
			{
				isShooting = false;
				possessor = receiver;
			}

			return option;
		}
	}
}