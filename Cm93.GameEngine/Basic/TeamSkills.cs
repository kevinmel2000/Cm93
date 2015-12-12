﻿/*
        Copyright © Iain McDonald 2013-2015
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
using Cm93.Model.Structures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cm93.GameEngine.Basic
{
	public class TeamSkills
	{
		private const double Flatten = 1d;

		private IList<Player> HomeTeamFormation { get; set; }
		private IList<Player> AwayTeamFormation { get; set; }

		private static readonly Func<Coordinate, Coordinate, double, double> Distribution = (position, player, rating) =>
			rating * (Math.Exp(-((player.X - position.X) * (player.X - position.X) + (player.Y - position.Y) * (player.Y - position.Y)) / Flatten));

		public Func<Coordinate, double> HomeTeamPace { get; private set; }
		public Func<Coordinate, double> HomeTeamPassing { get; private set; }
		public Func<Coordinate, double> HomeTeamTackling { get; private set; }
		public Func<Coordinate, double> HomeTeamShooting { get; private set; }
		public Func<Coordinate, double> HomeTeamDribbling { get; private set; }

		public Func<Coordinate, double> AwayTeamPace { get; private set; }
		public Func<Coordinate, double> AwayTeamPassing { get; private set; }
		public Func<Coordinate, double> AwayTeamTackling { get; private set; }
		public Func<Coordinate, double> AwayTeamShooting { get; private set; }
		public Func<Coordinate, double> AwayTeamDribbling { get; private set; }

		public TeamSkills(IList<Player> homeTeamFormation, IList<Player> awayTeamFormation)
		{
			HomeTeamFormation = homeTeamFormation;
			AwayTeamFormation = awayTeamFormation;

			CreateSkillLambdaFunctions();
		}

		private void CreateSkillLambdaFunctions()
		{
			HomeTeamPace = coordinate => HomeTeamFormation.Select(p => Distribution(coordinate, p.Location, p.Rating)).Sum();
			HomeTeamPassing = coordinate => HomeTeamFormation.Select(p => Distribution(coordinate, p.Location, p.Rating)).Sum();
			HomeTeamTackling = coordinate => HomeTeamFormation.Select(p => Distribution(coordinate, p.Location, p.Rating)).Sum();
			HomeTeamShooting = coordinate => HomeTeamFormation.Select(p => Distribution(coordinate, p.Location, p.Rating)).Sum();
			HomeTeamDribbling = coordinate => HomeTeamFormation.Select(p => Distribution(coordinate, p.Location, p.Rating)).Sum();

			AwayTeamPace = coordinate => AwayTeamFormation.Select(p => Distribution(coordinate, p.Location, p.Rating)).Sum();
			AwayTeamPassing = coordinate => AwayTeamFormation.Select(p => Distribution(coordinate, p.Location, p.Rating)).Sum();
			AwayTeamTackling = coordinate => AwayTeamFormation.Select(p => Distribution(coordinate, p.Location, p.Rating)).Sum();
			AwayTeamShooting = coordinate => AwayTeamFormation.Select(p => Distribution(coordinate, p.Location, p.Rating)).Sum();
			AwayTeamDribbling = coordinate => AwayTeamFormation.Select(p => Distribution(coordinate, p.Location, p.Rating)).Sum();
		}
	}
}
