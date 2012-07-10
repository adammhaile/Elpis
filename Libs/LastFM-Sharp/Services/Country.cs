//  
//  Copyright (C) 2009 Amr Hassan
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Xml;
using System.Collections.Generic;

namespace Lastfm.Services
{
	/// <summary>
	/// A country on Last.fm.
	/// </summary>
	public class Country : Base, IHasURL
	{
		/// <value>
		/// The country's name.
		/// </value>
		public string Name {get; private set;}
		
		/// <summary>
		/// A country by its name or its ISO 3166-1 alpha-2 code.
		/// </summary>
		/// <param name="name">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="session">
		/// A <see cref="Session"/>
		/// </param>
		public Country(string name, Session session)
			:base(session)
		{
			if(name.Length == 2)
				Name = getName(name);
			else
				Name = name;
		}
		
		internal override RequestParameters getParams ()
		{
			RequestParameters p = new Lastfm.RequestParameters();
			p["country"] = Name;
			
			return p;
		}
		
		/// <summary>
		/// String representation of the object.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public override string ToString()
		{
			return Name;
		}
		
		private string getName(string alpha2code)
		{
			Dictionary<string, string> codes = new Dictionary<string, string>();
			
			codes.Add("AD", "Andorra");
			codes.Add("AE", "United Arab Emirates");
			codes.Add("AF", "Afghanistan");
			codes.Add("AG", "Antigua and Barbuda");
			codes.Add("AI", "Anguilla");
			codes.Add("AL", "Albania");
			codes.Add("AM", "Armenia");
			codes.Add("AN", "Netherlands Antilles");
			codes.Add("AO", "Angola");
			codes.Add("AQ", "Antarctica");
			codes.Add("AR", "Argentina");
			codes.Add("AS", "American Samoa");
			codes.Add("AT", "Austria");
			codes.Add("AU", "Australia");
			codes.Add("AW", "Aruba");
			codes.Add("AX", "Åland Islands");
			codes.Add("AZ", "Azerbaijan");
			codes.Add("BA", "Bosnia and Herzegovina");
			codes.Add("BB", "Barbados");
			codes.Add("BD", "Bangladesh");
			codes.Add("BE", "Belgium");
			codes.Add("BF", "Burkina Faso");
			codes.Add("BG", "Bulgaria");
			codes.Add("BH", "Bahrain");
			codes.Add("BI", "Burundi");
			codes.Add("BJ", "Benin");
			codes.Add("BM", "Bermuda");
			codes.Add("BN", "Brunei Darussalam");
			codes.Add("BO", "Bolivia");
			codes.Add("BR", "Brazil");
			codes.Add("BS", "Bahamas");
			codes.Add("BT", "Bhutan");
			codes.Add("BV", "Bouvet Island");
			codes.Add("BW", "Botswana");
			codes.Add("BY", "Belarus");
			codes.Add("BZ", "Belize");
			codes.Add("CA", "Canada");
			codes.Add("CC", "Cocos (Keeling) Islands");
			codes.Add("CD", "Congo, Democratic Republic of the");
			codes.Add("CF", "Central African Republic");
			codes.Add("CG", "Congo");
			codes.Add("CH", "Switzerland");
			codes.Add("CI", "Côte d'Ivoire");
			codes.Add("CK", "Cook Islands");
			codes.Add("CL", "Chile");
			codes.Add("CM", "Cameroon");
			codes.Add("CN", "China");
			codes.Add("CO", "Colombia");
			codes.Add("CR", "Costa Rica");
			codes.Add("CS", "Serbia");
			codes.Add("CU", "Cuba");
			codes.Add("CV", "Cape Verde");
			codes.Add("CX", "Christmas Island");
			codes.Add("CY", "Cyprus");
			codes.Add("CZ", "Czech Republic");
			codes.Add("DE", "Germany");
			codes.Add("DJ", "Djibouti");
			codes.Add("DK", "Denmark");
			codes.Add("DM", "Dominica");
			codes.Add("DO", "Dominican Republic");
			codes.Add("DZ", "Algeria");
			codes.Add("EC", "Ecuador");
			codes.Add("EE", "Estonia");
			codes.Add("EG", "Egypt");
			codes.Add("EH", "Western Sahara");
			codes.Add("ER", "Eritrea");
			codes.Add("ES", "Spain");
			codes.Add("ET", "Ethiopia");
			codes.Add("FI", "Finland");
			codes.Add("FJ", "Fiji");
			codes.Add("FK", "Falkland Islands");
			codes.Add("FM", "Micronesia, Federated States of");
			codes.Add("FO", "Faroe Islands");
			codes.Add("FR", "France");
			codes.Add("GA", "Gabon");
			codes.Add("GB", "United Kingdom");
			codes.Add("GD", "Grenada");
			codes.Add("GE", "Georgia");
			codes.Add("GF", "French Guiana");
			codes.Add("GH", "Ghana");
			codes.Add("GI", "Gibraltar");
			codes.Add("GL", "Greenland");
			codes.Add("GM", "Gambia");
			codes.Add("GN", "Guinea");
			codes.Add("GP", "Guadeloupe");
			codes.Add("GQ", "Equatorial Guinea");
			codes.Add("GR", "Greece");
			codes.Add("GS", "South Georgia and the South Sandwich Islands");
			codes.Add("GT", "Guatemala");
			codes.Add("GU", "Guam");
			codes.Add("GW", "Guinea-Bissau");
			codes.Add("GY", "Guyana");
			codes.Add("HK", "Hong Kong");
			codes.Add("HM", "Heard Island and McDonald Islands");
			codes.Add("HN", "Honduras");
			codes.Add("HR", "Croatia");
			codes.Add("HT", "Haiti");
			codes.Add("HU", "Hungary");
			codes.Add("ID", "Indonesia");
			codes.Add("IE", "Ireland");
			codes.Add("IL", "Israel");
			codes.Add("IN", "India");
			codes.Add("IO", "British Indian Ocean Territory");
			codes.Add("IQ", "Iraq");
			codes.Add("IR", "Iran, Islamic Republic of");
			codes.Add("IS", "Iceland");
			codes.Add("IT", "Italy");
			codes.Add("JM", "Jamaica");
			codes.Add("JO", "Jordan");
			codes.Add("JP", "Japan");
			codes.Add("KE", "Kenya");
			codes.Add("KG", "Kyrgyzstan");
			codes.Add("KH", "Cambodia");
			codes.Add("KI", "Kiribati");
			codes.Add("KM", "Comoros");
			codes.Add("KN", "Saint Kitts and Nevis");
			codes.Add("KP", "Korea, Democratic People's Republic of");
			codes.Add("KR", "Korea, Republic of");
			codes.Add("KW", "Kuwait");
			codes.Add("KY", "Cayman Islands");
			codes.Add("KZ", "Kazakhstan");
			codes.Add("LA", "Lao People's Democratic Republic");
			codes.Add("LB", "Lebanon");
			codes.Add("LC", "Saint Lucia");
			codes.Add("LI", "Liechtenstein");
			codes.Add("LK", "Sri Lanka");
			codes.Add("LR", "Liberia");
			codes.Add("LS", "Lesotho");
			codes.Add("LT", "Lithuania");
			codes.Add("LU", "Luxembourg");
			codes.Add("LV", "Latvia");
			codes.Add("LY", "Libyan Arab Jamahiriya (Libya)");
			codes.Add("MA", "Morocco");
			codes.Add("MC", "Monaco");
			codes.Add("MD", "Moldova");
			codes.Add("MG", "Madagascar");
			codes.Add("MH", "Marshall Islands");
			codes.Add("MK", "Macedonia");
			codes.Add("ML", "Mali");
			codes.Add("MM", "Myanmar (Burma)");
			codes.Add("MN", "Mongolia");
			codes.Add("MO", "Macao (Macau)");
			codes.Add("MP", "Northern Mariana Islands");
			codes.Add("MQ", "Martinique");
			codes.Add("MR", "Mauritania");
			codes.Add("MS", "Montserrat");
			codes.Add("MT", "Malta");
			codes.Add("MU", "Mauritius");
			codes.Add("MV", "Maldives");
			codes.Add("MW", "Malawi");
			codes.Add("MX", "Mexico");
			codes.Add("MY", "Malaysia");
			codes.Add("MZ", "Mozambique");
			codes.Add("NA", "Namibia");
			codes.Add("NC", "New Caledonia");
			codes.Add("NE", "Niger");
			codes.Add("NF", "Norfolk Island");
			codes.Add("NG", "Nigeria");
			codes.Add("NI", "Nicaragua");
			codes.Add("NL", "Netherlands");
			codes.Add("NO", "Norway");
			codes.Add("NP", "Nepal");
			codes.Add("NR", "Nauru");
			codes.Add("NU", "Niue");
			codes.Add("NZ", "New Zealand");
			codes.Add("OM", "Oman");
			codes.Add("PA", "Panama");
			codes.Add("PE", "Peru");
			codes.Add("PF", "French Polynesia");
			codes.Add("PG", "Papua New Guinea");
			codes.Add("PH", "Philippines");
			codes.Add("PK", "Pakistan");
			codes.Add("PL", "Poland");
			codes.Add("PM", "Saint-Pierre and Miquelon");
			codes.Add("PN", "Pitcairn Islands");
			codes.Add("PR", "Puerto Rico");
			codes.Add("PS", "Palestinian Territory, Occupied");
			codes.Add("PT", "Portugal");
			codes.Add("PW", "Palau");
			codes.Add("PY", "Paraguay");
			codes.Add("QA", "Qatar");
			codes.Add("RE", "Réunion");
			codes.Add("RO", "Romania");
			codes.Add("RU", "Russia");
			codes.Add("RW", "Rwanda");
			codes.Add("SA", "Saudi Arabia");
			codes.Add("SB", "Solomon Islands");
			codes.Add("SC", "Seychelles");
			codes.Add("SD", "Sudan");
			codes.Add("SE", "Sweden");
			codes.Add("SG", "Singapore");
			codes.Add("SH", "Saint Helena");
			codes.Add("SI", "Slovenia");
			codes.Add("SJ", "Svalbard and Jan Mayen Islands");
			codes.Add("SK", "Slovakia");
			codes.Add("SL", "Sierra Leone");
			codes.Add("SM", "San Marino");
			codes.Add("SN", "Senegal");
			codes.Add("SO", "Somalia");
			codes.Add("SR", "Suriname");
			codes.Add("ST", "São Tomé and Príncipe");
			codes.Add("SV", "El Salvador");
			codes.Add("SY", "Syrian Arab Republic");
			codes.Add("SZ", "Swaziland");
			codes.Add("TC", "Turks and Caicos Islands");
			codes.Add("TD", "Chad");
			codes.Add("TF", "French Southern Territories");
			codes.Add("TG", "Togo");
			codes.Add("TH", "Thailand");
			codes.Add("TJ", "Tajikistan");
			codes.Add("TK", "Tokelau");
			codes.Add("TL", "Timor-Leste");
			codes.Add("TM", "Turkmenistan");
			codes.Add("TN", "Tunisia");
			codes.Add("TO", "Tonga");
			codes.Add("TR", "Turkey");
			codes.Add("TT", "Trinidad and Tobago");
			codes.Add("TV", "Tuvalu");
			codes.Add("TW", "Taiwan");
			codes.Add("TZ", "Tanzania, United Republic of");
			codes.Add("UA", "Ukraine");
			codes.Add("UG", "Uganda");
			codes.Add("UM", "United States Minor Outlying Islands");
			codes.Add("US", "United States");
			codes.Add("UY", "Uruguay");
			codes.Add("UZ", "Uzbekistan");
			codes.Add("VA", "Holy See");
			codes.Add("VC", "Saint Vincent and the Grenadines");
			codes.Add("VE", "Venezuela");
			codes.Add("VG", "Virgin Islands, British");
			codes.Add("VI", "Virgin Islands, U.S.");
			codes.Add("VN", "Viet Nam");
			codes.Add("VU", "Vanuatu");
			codes.Add("WF", "Wallis and Futuna");
			codes.Add("WS", "Samoa");
			codes.Add("YE", "Yemen");
			codes.Add("YT", "Mayotte");
			codes.Add("ZA", "South Africa");
			codes.Add("ZM", "Zambia");
			codes.Add("ZW", "Zimbabwe");
			
			return codes[alpha2code];
		}
		
		public TopArtist[] GetTopArtists()
		{
			XmlDocument doc = request("geo.getTopArtists");
			
			List<TopArtist> list = new List<TopArtist>();
			foreach(XmlNode node in doc.GetElementsByTagName("artist"))
				list.Add(new TopArtist(new Artist(extract(node, "name"), Session),
				                       Int32.Parse(extract(node, "playcount"))));
			
			return list.ToArray();
		}
		
		public TopArtist[] GetTopArtists(int limit)
		{
			return sublist<TopArtist>(GetTopArtists(), limit);
		}
		
		public TopTrack[] GetTopTracks()
		{
			XmlDocument doc = request("geo.getTopTracks");
			
			List<TopTrack> list = new List<TopTrack>();
			foreach(XmlNode node in doc.GetElementsByTagName("track"))
			{
				Track track = new Track(extract(node, "name", 1), extract(node, "name"), Session);
				int playcount = Int32.Parse(extract(node, "playcount"));
				
				list.Add(new TopTrack(track, playcount));
			}
			
			return list.ToArray();
		}
		
		public TopTrack[] GetTopTracks(int limit)
		{
			return sublist<TopTrack>(GetTopTracks(), limit);
		}
		
		/// <summary>
		/// Returns the country's page url on Last.fm.
		/// </summary>
		/// <param name="language">
		/// A <see cref="SiteLanguage"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string GetURL(SiteLanguage language)
		{
			string domain = getSiteDomain(language);
			
			return "http://" + domain + "/place/" + urlSafe(Name);
		}
		
		/// <value>
		/// The country's page url on Last.fm.
		/// </value>
		public string URL {
			get { return this.GetURL(SiteLanguage.English); }
		}


	}
}
