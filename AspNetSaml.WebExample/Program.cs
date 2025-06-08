using Microsoft.AspNetCore.Mvc;

// TODO: specify the certificate that your SAML provider gave you
// your app's entity ID
// and the SAML provider's endpoint (where we should redirect the user)
const string SAML_CERTIFICATE = """
-----BEGIN CERTIFICATE-----
MIIE6DCCAtCgAwIBAgIQOQaY6KUdPItB52hpOsIBvjANBgkqhkiG9w0BAQsFADAw
MS4wLAYDVQQDEyVBREZTIFNpZ25pbmcgLSBzc28uc2F0aXRtLmNodWxhLmFjLnRo
MB4XDTI1MDQwNjEyNTQxNFoXDTI2MDQwNjEyNTQxNFowMDEuMCwGA1UEAxMlQURG
UyBTaWduaW5nIC0gc3NvLnNhdGl0bS5jaHVsYS5hYy50aDCCAiIwDQYJKoZIhvcN
AQEBBQADggIPADCCAgoCggIBAMlu6kjF9Ghsr9Z6+AIYRjHTx4OL6fROrCzq26/h
YBfsrsL5QeJlWtYhRsbrW3wAFaQukNYal5LRJx8BXXlngIDIfoIEixT62BqFC2XO
Ju7Rq+p1ei2WZb06V0It8ohmZVPqsDPzygjBblta27DBGQ8qQ4upGVTwOIBRisMj
Ixxx90p6DeB2ZCiGOYCYMYPdFWwz8QCZv64WbWRw3WhRKla05nyiV352aaC53XL0
ZZlRFV8jj6YiKsbKEzkxKpDVxEaH28NGVptBJyfkU5VOpqkmZZtqhSCrrIprfa+j
Dl6De9Siq8/CUDoZhkhRoNUqmhaiu0ZbV3AF0iN+XLtmeP/GJREz5m3gOoAGH8Rl
g5pyca6vmSnJHKnTsu8Elc4pVvO6jH1hqdBLVFa4uftqqBY2B/ZuUXj7764eHsMZ
kHZC6SXOxAP2BPbRbslbd4CRErnuE5rgMRQAYQVWcrDvagUdvm2T1+wJN7GmwBg9
GGhTA3r9howvIj2RFLxCZbpy2QlWKMb1zjyvtCHrM7g8/aGuvJfY5cmfww5aib4a
QpJq+ZyCPZpW8iXZTnxVuyV57WFTOmCvy/9dfK/IQXEqG5FIikwaB2nyL/D5FXIP
xH+OzLeLdLlKe2zpOJgx2p1M6rJ29AJRASKs+ikqlSV/i5t+1sw2qinFKJ8ZegsN
HDgBAgMBAAEwDQYJKoZIhvcNAQELBQADggIBABzrTEbbzMHbq0mIV1w3TL6IVOv8
BeXoYznSI7P/MhJwBXMbrYNNbpSkv5jWhtSAWQWrDrN0IUqvKwIYYRlRtgvma6Mk
PFXRvzkVhpuqm/bp1HAH2yoJUXNuWInzdJeMnPaQymU/hSvSJ8f66pwlPrAYTSBk
YIbcEdLJ3OmcnjOjj4W+s70J0s0HTnNQboAzjue3SmpsPVVetP+cwaoIASz2M6Fr
wfqFaUUiSAxcUzfELOyN8d1dnRFQVkrNyayz0fHH2Kje5GnGLMNaZTKZ88nVbmoq
Cbow7ofjEb74jNwWhmRhntuEE0I1W55LnU3Srjptjnfkd604W60DbqiWBKM6rTTb
ilCJsW3umI26/eFZAfZIA2n7/FKDcDXFCJOM1UV+09pZ12p0TAaA3nyA2TbdI+PM
GtvvEK2PU+tdU64uAlOOaldk52dIuR7kOVBk53Gf3K2wY2U/oovLrlXLHb8NJD49
Po6XT3w6WL+okyr7FgdmAHTNpTnthXG0pyN4KUFEAK9HWXdGRWUgFX4yBOjmPN1N
Vx7G3klMd+ccQUU80lxDKQUbjhcWLloWNlg6w2SKk4Ku7/f8HmPkqppvFow+ytWm
0abcCjptoUrFR1BCM86CIDCo9bEWIyWv+SHr2AvlQ8D0Z8aQRr6M73NN1PNWruPf
bpD2ekhB2vZ9R2ij
-----END CERTIFICATE-----
""";
const string ENTITY_ID = "https://cudreg.com";
const string SAML_ENDPOINT = "https://sso.satitm.chula.ac.th/adfs/ls";

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseHttpsRedirection();


//homepage
app.MapGet("/", () =>
{
	var request = new Saml.AuthRequest(
		ENTITY_ID,
		"https://localhost:7009/cudreg/adfs/postresponse"
	);

	//now send the user to the SAML provider
	var url = request.GetRedirectUrl(SAML_ENDPOINT);
	
	return Results.Content("Click <a href=\"" + url + "\">here</a> to log in", "text/html");
}).DisableAntiforgery();


app.MapPost("/cudreg/adfs/postresponse", ([FromForm] string samlResponse) =>
{
    try 
    {
        var saml = new Saml.Response(SAML_CERTIFICATE, samlResponse);

        if (saml.IsValid()) // SAML response is valid
        {
            // Email: For Student, sXXXXX@satitm.chula.ac.th, XXXXX is the student ID
            var email = saml.GetEmail();
            var firstName = saml.GetFirstName();
            var lastName = saml.GetLastName();
            return Results.Content($"Success! Logged in as user {email} ({firstName} {lastName})", "text/html", System.Text.Encoding.UTF8);
        }
        else 
        {
            return Results.Content("SAML validation failed: Response is not valid.", "text/html");
        }
    }
    catch (Exception ex)
    {
        return Results.Content("Login Error", "text/html", System.Text.Encoding.UTF8);
    }
}).DisableAntiforgery();



//IdP will send logout requests here
app.MapPost("/cudreg/adfs/logout", ([FromForm] string samlResponse) =>
{
	var saml = new Saml.IdpLogoutRequest(SAML_CERTIFICATE, samlResponse);

	if (saml.IsValid()) //all good?
	{
        // Logout the user from your application here
    }

    return Results.Ok();
}).DisableAntiforgery();

app.Run();