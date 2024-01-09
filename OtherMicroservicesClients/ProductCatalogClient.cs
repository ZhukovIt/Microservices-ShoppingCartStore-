using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http.Headers;
using ShoppingCartLogic.ShoppingCarts;
using CSharpFunctionalExtensions;
using FluentNHibernate.Conventions.Helpers;
using MicroservicesCommon.ValueObjects;
using ProductCatalogLogic.ProductCatalog;
using MicroservicesCommon.Api;
using Newtonsoft.Json;
using FluentNHibernate.Testing.Values;

namespace Api.OtherMicroservicesClients
{
    public class ProductCatalogClient
    {
        private readonly HttpClient _client;
        private readonly ProductCatalogCache _cache;
        private const string _productsRoute = "/products";

        public ProductCatalogClient(HttpClient _Client, ProductCatalogCache _Cache)
        {
            _client = _Client;
            _cache = _Cache;
        }

        public async Task<Result<IEnumerable<ShoppingCartItem>>> GetShoppingCartItemsAsync(long[] productCatalogIds)
        {
            Result<IEnumerable<ProductDto>> productsOrError = await TryGetProductsByIDs(productCatalogIds, _client.BaseAddress + _productsRoute);
            if (productsOrError.IsFailure)
                return Result.Failure<IEnumerable<ShoppingCartItem>>(productsOrError.Error);

            return Result.Success(productsOrError.Value
                .Select(p => 
                    new ShoppingCartItem(
                        p.Id, 
                        (ProductName)p.Name, 
                        ProductDescription.CreateNullable(p.Description).Value, 
                        Money.Create(p.Amount, p.Currency).Value)));
        }

        private async Task<Result<IEnumerable<ProductDto>>> TryGetProductsByIDs(IEnumerable<long> _ProductIDs, string _RequestRoute)
        {
            List<ProductDto> result = new List<ProductDto>();
            List<long> requestedProductIDs = new List<long>();

            foreach (long id in _ProductIDs)
            {
                Maybe<Product> productOrDefault = _cache.Get(id);
                if (productOrDefault.HasValue)
                {
                    result.Add(new ProductDto(productOrDefault.Value));
                    continue;
                }

                requestedProductIDs.Add(id);
            }

            if (requestedProductIDs.Count == 0)
                return result;

            JsonContent content = JsonContent.Create(requestedProductIDs);

            HttpResponseMessage response = await _client.PostAsync(_RequestRoute, content);

            string serializedEnvelope = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Envelope failureEnvelope = JsonConvert.DeserializeObject<Envelope>(serializedEnvelope)!;
                return Result.Failure<IEnumerable<ProductDto>>(failureEnvelope.ErrorMessage);
            }

            Envelope<List<ProductDto>> successEnvelope = JsonConvert.DeserializeObject<Envelope<List<ProductDto>>>(serializedEnvelope)!;
            AddToCache(response, successEnvelope.Result);

            return Result.Success(result.Union(successEnvelope.Result));
        }

        private void AddToCache(HttpResponseMessage _Response, List<ProductDto> _Products)
        {
            KeyValuePair<string, IEnumerable<string>> cacheHeader = _Response.Headers
                .FirstOrDefault(h => h.Key == "cache-control");
            if (string.IsNullOrWhiteSpace(cacheHeader.Key))
                return;

            if (CacheControlHeaderValue.TryParse(cacheHeader.Value.ToString(), out CacheControlHeaderValue? headerValue) &&
                headerValue?.MaxAge != null)
            {
                foreach (ProductDto productDto in _Products)
                {
                    Result<ProductName> productNameOrError = ProductName.Create(productDto.Name);
                    Result<Maybe<ProductDescription>> productDescriptionOrError = ProductDescription.CreateNullable(productDto.Description);
                    Result<Money> moneyOrError = Money.Create(productDto.Amount, productDto.Currency);
                    Result resultOrError = Result.Combine(productNameOrError, productDescriptionOrError, moneyOrError);
                    if (resultOrError.IsFailure)
                        continue;

                    Product product = new Product(productDto.Id, productNameOrError.Value, productDescriptionOrError.Value, moneyOrError.Value);

                    _cache.Add(productDto.Id, product, headerValue.MaxAge.Value);
                }
            }
        }
    }
}

