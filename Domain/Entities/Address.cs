﻿namespace Domain.Entities
{
	public class Address
    {
        public int Id { get; set; }
        public string City { get; set; }    
        public string Street { get; set; }    
        public string PostalCode { get; set; }
        public List<Restaurant> Restaurant { get; set; }
    }
}